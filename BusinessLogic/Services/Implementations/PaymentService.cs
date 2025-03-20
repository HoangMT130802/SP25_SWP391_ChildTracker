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
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _payOS = payOS;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            try
            {
                // 1. Kiểm tra user và membership
                var userRepo = _unitOfWork.GetRepository<User>();
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();

                var user = await userRepo.GetAsync(u => u.UserId == request.UserId);
                if (user == null)
                    throw new Exception("Không tìm thấy user");

                var membership = await membershipRepo.GetAsync(m => m.MembershipId == request.MembershipId);
                if (membership == null)
                    throw new Exception("Không tìm thấy gói membership");

                // 2. Kiểm tra user đã có membership active chưa
                var activeMembership = await userMembershipRepo.GetAsync(
                    um => um.UserId == request.UserId && um.Status == "Active"
                );
                if (activeMembership != null)
                    throw new Exception("User đã có membership đang active");

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
                    null // Không cần webhook URL
                );

                var createPayment = await _payOS.createPaymentLink(paymentData);

                var paymentStatus = await _payOS.getPaymentLinkInformation(createPayment.orderCode);

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
                _logger.LogError(ex, "Lỗi khi tạo payment");
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

                    if (existingTransaction == null)
                    {
                        // Tạo transaction mới
                        var newTransaction = new DataAccess.Entities.Transaction
                        {
                            UserId = userId,
                            UserMembershipId = membershipId,
                            Amount = paymentInfo.amount,
                            PaymentMethod = "PayOS",
                            TransactionCode = orderId,
                            Description = $"Thanh toán gói membership",
                            Status = "PAID",
                            CreatedAt = DateTime.UtcNow
                        };

                        await transactionRepo.AddAsync(newTransaction);

                        // Tạo UserMembership mới
                        var userMembership = new UserMembership
                        {
                            UserId = userId,
                            MembershipId = membershipId,
                            Status = "Active",
                            StartDate = DateTime.UtcNow,
                            EndDate = DateTime.UtcNow.AddMonths(1),
                            LastRenewalDate = DateTime.UtcNow
                        };

                        await userMembershipRepo.AddAsync(userMembership);

                        // Cập nhật role user
                        var user = await userRepo.GetAsync(u => u.UserId == userId);
                        if (user != null)
                        {
                            user.Role = "Member";
                        }

                        await _unitOfWork.SaveChangesAsync();
                    }
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
                _logger.LogError(ex, "Lỗi khi kiểm tra trạng thái thanh toán");
                throw;
            }
            finally
            {
                await transaction.CommitAsync();
            }
        }
    }
}