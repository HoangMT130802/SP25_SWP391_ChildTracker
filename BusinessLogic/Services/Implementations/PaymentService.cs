using BusinessLogic.DTOs.Payment;
using BusinessLogic.DTOs.UserMembership;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using AutoMapper;
using Net.payOS;
using Net.payOS.Types;
using Microsoft.AspNetCore.Http;

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
                // Kiểm tra membership
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var membership = await membershipRepo.GetAsync(
                    m => m.MembershipId == request.MembershipId,
                    includeProperties: ""
                );

                if (membership == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy gói membership với ID {request.MembershipId}");
                }

                // Tạo order code unique
                long orderCode = long.Parse(DateTimeOffset.Now.ToString("ffffff"));

                // Tạo item data cho PayOS
                var item = new ItemData(
                    $"Gói {membership.Name}",
                    1,
                    (int)membership.Price // Cast decimal to int cho PayOS
                );
                var items = new List<ItemData> { item };

                // Get base URL cho redirect
                var baseUrl = "http://localhost:5177";

                // Tạo payment data
                var paymentData = new PaymentData(
                    orderCode,
                    (int)membership.Price, // Cast decimal to int cho PayOS
                    $"Thanh toán gói {membership.Name}",
                    items,
                    $"{baseUrl}/payment/success",
                    $"{baseUrl}/payment/cancel"
                );

                // Gọi PayOS để tạo payment link
                var createPayment = await _payOS.createPaymentLink(paymentData);

                // Tạo transaction pending
                var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                await transactionRepo.AddAsync(new DataAccess.Entities.Transaction
                {
                    UserId = request.UserId,
                    Amount = membership.Price,
                    PaymentMethod = "PayOS",
                    TransactionCode = orderCode.ToString(),
                    Description = $"Thanh toán gói {membership.Name}",
                    CreatedAt = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new PaymentResponseDTO
                {
                    PaymentUrl = createPayment.checkoutUrl,
                    OrderId = orderCode.ToString(),
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
                    // Parse orderID để lấy thông tin
                    var orderParts = webhookData.OrderId.Split('_');
                    int userId = int.Parse(orderParts[1]);
                    int membershipId = int.Parse(orderParts[2]);

                    // Lấy thông tin membership để set số lượt tư vấn
                    var membershipRepo = _unitOfWork.GetRepository<Membership>();
                    var membership = await membershipRepo.GetAsync(m => m.MembershipId == membershipId);
                    if (membership == null)
                    {
                        throw new KeyNotFoundException($"Không tìm thấy gói membership với ID {membershipId}");
                    }

                    // Tạo UserMembership
                    var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
                    var userMembership = new UserMembership
                    {
                        UserId = userId,
                        MembershipId = membershipId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(membership.Duration),
                        Status = "Active",
                        RemainingConsultations = membership.MaxConsultations,
                        LastRenewalDate = DateTime.UtcNow
                    };
                    await userMembershipRepo.AddAsync(userMembership);

                    // Cập nhật transaction
                    var transactionRepo = _unitOfWork.GetRepository<DataAccess.Entities.Transaction>();
                    var existingTransaction = await transactionRepo.GetAsync(
                        t => t.TransactionCode == webhookData.OrderId
                    );

                    if (existingTransaction != null)
                    {
                        existingTransaction.UserMembershipId = userMembership.UserMembershipId;
                        transactionRepo.Update(existingTransaction);
                    }

                    await _unitOfWork.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                    return true;
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
    }
}