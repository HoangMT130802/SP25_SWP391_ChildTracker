using BusinessLogic.DTOs.Payment;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Net.payOS;
using Net.payOS.Types;
using Microsoft.AspNetCore.Http;
using BusinessLogic.DTOs;

namespace BusinessLogic.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PayOS _payOS;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            PayOS payOS,
            IMapper mapper,
            ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _payOS = payOS;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Kiểm tra user tồn tại
                var userRepo = _unitOfWork.GetRepository<User>();
                var user = await userRepo.GetAsync(u => u.UserId == request.UserId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy user với ID {request.UserId}");
                }

                // Kiểm tra membership
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var membership = await membershipRepo.GetAsync(m => m.MembershipId == request.MembershipId);
                if (membership == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy gói membership với ID {request.MembershipId}");
                }

                // Tạo UserMembership trước với trạng thái Pending
                var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
                var userMembership = new UserMembership
                {
                    UserId = request.UserId,
                    MembershipId = request.MembershipId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(membership.Duration),
                    Status = "Pending",
                    RemainingConsultations = membership.MaxConsultations,
                    LastRenewalDate = DateTime.UtcNow
                };
                await userMembershipRepo.AddAsync(userMembership);
                await _unitOfWork.SaveChangesAsync();

                // Tạo order code
                string orderCode = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{request.UserId}_{request.MembershipId}";

                // Tạo transaction
                var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                var transaction = new DataAccess.Entities.Transaction
                {
                    UserId = request.UserId,
                    Amount = membership.Price,
                    PaymentMethod = "PayOS",
                    TransactionCode = orderCode,
                    Description = $"Thanh toán gói {membership.Name}",
                    CreatedAt = DateTime.UtcNow
                };
                await transactionRepo.AddAsync(transaction);

                // Tạo PayOS payment
                var item = new ItemData(
                    $"Gói {membership.Name}",
                    1,
                    (int)membership.Price
                );
                var items = new List<ItemData> { item };

                // Thay đổi URL webhook thành URL thực tế của bạn
                var baseUrl = "http://localhost:5175"; // Thay đổi thành domain thực tế của bạn
                var paymentData = new PaymentData(
                    long.Parse(orderCode.Split('_')[0]),
                    (int)membership.Price,
                    $"Thanh toán gói {membership.Name}",
                    items,
                    $"{baseUrl}/payment/cancel", // URL khi người dùng hủy thanh toán
                    $"{baseUrl}/payment/success", // URL khi thanh toán thành công
                    $"{baseUrl}/api/payment/webhook" // URL webhook để PayOS gọi về
                );

                var createPayment = await _payOS.createPaymentLink(paymentData);

                await _unitOfWork.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new PaymentResponseDTO
                {
                    PaymentUrl = createPayment.checkoutUrl,
                    OrderId = orderCode,
                    Amount = membership.Price,
                    Status = "PENDING",
                    Description = $"Thanh toán gói {membership.Name}"
                };
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo payment");
                throw;
            }
        }


        public async Task<bool> HandlePaymentWebhookAsync(PaymentWebhookDTO webhookData)
        {
            using var dbTransaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var paymentInfo = await _payOS.getPaymentLinkInformation(long.Parse(webhookData.OrderId));

                if (paymentInfo.status.Equals("PAID"))
                {
                    var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                    var transaction = await transactionRepo.GetAsync(
                        t => t.TransactionCode == webhookData.OrderId,
                        includeProperties: "UserMembership,UserMembership.Membership"
                    );

                    if (transaction != null)
                    {
                        var userMembership = transaction.UserMembership;
                        var membership = userMembership.Membership;

                        // Cập nhật UserMembership
                        userMembership.Status = "Active";
                        userMembership.StartDate = DateTime.UtcNow;
                        userMembership.EndDate = DateTime.UtcNow.AddDays(membership.Duration);
                        userMembership.RemainingConsultations = membership.MaxConsultations;
                        userMembership.LastRenewalDate = DateTime.UtcNow;

                        // Cập nhật Role của User
                        var userRepo = _unitOfWork.GetRepository<User>();
                        var user = await userRepo.GetAsync(u => u.UserId == transaction.UserId);
                        if (user != null)
                        {
                            user.Role = "Member";
                            userRepo.Update(user);
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await dbTransaction.CommitAsync();
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xử lý webhook");
                return false;
            }
        }
        public async Task<bool> CancelPayment(long orderCode)
        {
            try
            {
                var paymentInfo = await _payOS.cancelPaymentLink(orderCode);

                if (paymentInfo != null)
                {
                    // Cập nhật transaction nếu cần
                    var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                    var existingTransaction = await transactionRepo.GetAsync(
                        t => t.TransactionCode == orderCode.ToString()
                    );

                    if (existingTransaction != null)
                    {
                        existingTransaction.Description += " (Đã hủy)";
                        transactionRepo.Update(existingTransaction);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy payment");
                return false;
            }
        }

        public async Task<bool> VerifyPaymentAsync(string orderId, decimal amount, string checksum)
        {
            try
            {
                var paymentInfo = await _payOS.getPaymentLinkInformation(long.Parse(orderId));
                return paymentInfo.status.Equals("PAID");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi verify payment");
                return false;
            }
        }

        // Xử lý transactioc ở đây luôn
        public async Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync(int userId)
        {
            try
            {
                var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                var transactions = await transactionRepo.FindAsync(
                    t => t.UserId == userId,
                    includeProperties: "UserMembership,UserMembership.Membership"
                );

                var sortedTransactions = transactions.OrderByDescending(t => t.CreatedAt);
                return _mapper.Map<IEnumerable<TransactionDTO>>(sortedTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử giao dịch của user {UserId}", userId);
                throw;
            }
        }
        public async Task<TransactionDTO> GetTransactionByIdAsync(int transactionId)
        {
            try
            {
                var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                var transaction = await transactionRepo.GetAsync(
                    t => t.TransactionId == transactionId,
                    includeProperties: "UserMembership,UserMembership.Membership"
                );

                if (transaction == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy giao dịch với ID {transactionId}");
                }

                return _mapper.Map<TransactionDTO>(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin giao dịch {TransactionId}", transactionId);
                throw;
            }
        }
    }
}