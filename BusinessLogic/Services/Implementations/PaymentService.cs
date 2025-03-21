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
using System.Buffers.Text;
using Microsoft.Extensions.Configuration;

namespace BusinessLogic.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;
        private readonly PayOS _payOS;
        private const string BASE_URL = "http://localhost:5175";

        public PaymentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<PaymentService> logger,
            PayOS payOS)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _payOS = payOS ?? throw new ArgumentNullException(nameof(payOS));
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogInformation("Bắt đầu tạo payment cho user {UserId}", request.UserId);

                // 1. Kiểm tra user và membership
                var userRepo = _unitOfWork.GetRepository<User>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
                var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();

                // Kiểm tra user
                var user = await userRepo.GetAsync(u => u.UserId == request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Không tìm thấy user với ID: {UserId}", request.UserId);
                    throw new Exception("Không tìm thấy user");
                }

                // Kiểm tra membership
                var membership = await membershipRepo.GetAsync(m => m.MembershipId == request.MembershipId);
                if (membership == null)
                {
                    _logger.LogWarning("Không tìm thấy membership với ID: {MembershipId}", request.MembershipId);
                    throw new Exception("Không tìm thấy gói membership");
                }

                // 2. Kiểm tra user đã có membership active chưa
                var activeMembership = await userMembershipRepo.GetAsync(
                    um => um.UserId == request.UserId && um.Status == "Active"
                );
                if (activeMembership != null)
                {
                    _logger.LogWarning("User {UserId} đã có membership active", request.UserId);
                    throw new Exception("User đã có membership đang active");
                }

                // 3. Tạo mã giao dịch
                string orderCode = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{request.UserId}_{request.MembershipId}";

                // 4. Tạo payment link
                var paymentData = new PaymentData(
                    long.Parse(orderCode.Split('_')[0]),
                    (int)membership.Price,
                    $"Thanh toán gói {membership.Name}",
                    new List<ItemData>(),
                    $"{BASE_URL}/payment/cancel",
                    $"{BASE_URL}/payment/success",
                    null
                );

                var createPayment = await _payOS.createPaymentLink(paymentData);

                // 5. Tạo UserMembership mới với trạng thái Pending
                var newUserMembership = new UserMembership
                {
                    UserId = request.UserId,
                    MembershipId = request.MembershipId,
                    Status = "Pending",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    LastRenewalDate = DateTime.UtcNow
                };

                await userMembershipRepo.AddAsync(newUserMembership);
                await _unitOfWork.SaveChangesAsync();

                // 6. Lưu transaction vào database với UserMembershipId mới
                var newTransaction = _mapper.Map<DataAccess.Entities.Transaction>(request);
                newTransaction.Amount = membership.Price;
                newTransaction.Description = $"Thanh toán gói {membership.Name}";
                newTransaction.TransactionCode = orderCode;
                newTransaction.UserMembershipId = newUserMembership.UserMembershipId;
                newTransaction.Status = "PENDING"; // Set transaction status to PENDING initially

                await transactionRepo.AddAsync(newTransaction);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Payment URL: {createPayment.checkoutUrl}");

                return new PaymentResponseDTO
                {
                    PaymentUrl = createPayment.checkoutUrl,
                    OrderId = orderCode,
                    Amount = membership.Price,
                    Status = "PENDING"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo payment cho user {UserId}. Chi tiết: {Message}", request?.UserId, ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                throw;
            }
        }

        public async Task<PaymentStatusDTO> CheckPaymentStatusAsync(string orderId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra status từ PayOS
                var paymentInfo = await _payOS.getPaymentLinkInformation(long.Parse(orderId.Split('_')[0]));

                // 2. Nếu thanh toán thành công
                if (paymentInfo.status.Equals("PAID"))
                {
                    var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                    var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
                    var userRepo = _unitOfWork.GetRepository<User>();

                    // Lấy thông tin từ orderId
                    var userId = int.Parse(orderId.Split('_')[1]);
                    var membershipId = int.Parse(orderId.Split('_')[2]);

                    // Kiểm tra transaction đã tồn tại chưa
                    var existingTransaction = await transactionRepo.GetAsync(
                        t => t.TransactionCode == orderId
                    );

                    if (existingTransaction != null)
                    {
                        // Cập nhật trạng thái transaction
                        existingTransaction.Status = "PAID";
                        existingTransaction.Amount = paymentInfo.amount;

                        // Tìm và cập nhật UserMembership tương ứng
                        var existingUserMembership = await userMembershipRepo.GetAsync(
                            um => um.UserMembershipId == existingTransaction.UserMembershipId
                        );

                        if (existingUserMembership != null)
                        {
                            existingUserMembership.Status = "Active";
                            existingUserMembership.StartDate = DateTime.UtcNow;
                            existingUserMembership.EndDate = DateTime.UtcNow.AddMonths(1);
                            existingUserMembership.LastRenewalDate = DateTime.UtcNow;

                            // Cập nhật role user
                            var user = await userRepo.GetAsync(u => u.UserId == userId);
                            if (user != null)
                            {
                                user.Role = "Member";
                            }
                        }

                        await _unitOfWork.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.CommitAsync();
                }

                return new PaymentStatusDTO
                {
                    Success = true,
                    Status = paymentInfo.status,
                    Message = paymentInfo.status.Equals("PAID") ? "Thanh toán thành công" :
                              paymentInfo.status.Equals("PENDING") ? "Đang chờ thanh toán" :
                              paymentInfo.status.Equals("CANCELLED") ? "Đã hủy" : "Không xác định"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi kiểm tra trạng thái thanh toán. Chi tiết: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
                }
                throw;
            }
        }
    }
}