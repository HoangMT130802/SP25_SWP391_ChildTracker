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

                // Kiểm tra xem user đã có membership active chưa
                var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
                var existingActiveMembership = await userMembershipRepo.GetAsync(
                    um => um.UserId == request.UserId &&
                          um.Status == "Active" &&
                          um.EndDate > DateTime.UtcNow
                );

                if (existingActiveMembership != null)
                {
                    throw new InvalidOperationException("Bạn đã có gói membership đang active. Vui lòng đợi gói hiện tại hết hạn.");
                }

                // Tạo order code
                string orderCode = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{request.UserId}_{request.MembershipId}";

                // Tạo transaction để lưu thông tin thanh toán
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

                var baseUrl = "http://localhost:5177";
                var paymentData = new PaymentData(
                    long.Parse(orderCode.Split('_')[0]),
                    (int)membership.Price,
                    $"Thanh toán gói {membership.Name}",
                    items,
                    $"{baseUrl}/payment/cancel",
                    $"{baseUrl}/payment/success"
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
                        t => t.TransactionCode == webhookData.OrderId
                    );

                    if (transaction != null)
                    {
                        // Lấy thông tin membership từ orderCode
                        var membershipId = int.Parse(webhookData.OrderId.Split('_')[2]);
                        var membershipRepo = _unitOfWork.GetRepository<Membership>();
                        var membership = await membershipRepo.GetAsync(m => m.MembershipId == membershipId);

                        if (membership != null)
                        {
                            // Tạo UserMembership mới với status Active
                            var userMembershipRepo = _unitOfWork.GetRepository<UserMembership>();
                            var userMembership = new UserMembership
                            {
                                UserId = transaction.UserId,
                                MembershipId = membershipId,
                                StartDate = DateTime.UtcNow,
                                EndDate = DateTime.UtcNow.AddDays(membership.Duration),
                                Status = "Active",
                                RemainingConsultations = membership.MaxConsultations,
                                LastRenewalDate = DateTime.UtcNow
                            };
                            await userMembershipRepo.AddAsync(userMembership);

                            // Cập nhật UserMembershipId trong transaction
                            transaction.UserMembershipId = userMembership.UserMembershipId;

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
    }
}