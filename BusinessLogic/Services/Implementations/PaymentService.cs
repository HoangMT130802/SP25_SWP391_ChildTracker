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

namespace BusinessLogic.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayOSConfig _payOSConfig;
        private readonly IUserMembershipService _userMembershipService;
        private readonly ILogger<PaymentService> _logger;
        private readonly HttpClient _httpClient;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IOptions<PayOSConfig> payOSConfig,
            IUserMembershipService userMembershipService,
            ILogger<PaymentService> logger,
            HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _payOSConfig = payOSConfig.Value;
            _userMembershipService = userMembershipService;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request)
        {
            try
            {
                // Lấy thông tin membership
                var membershipRepo = _unitOfWork.GetRepository<Membership>();
                var membership = await membershipRepo.GetAsync(m => m.MembershipId == request.MembershipId);
                if (membership == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy gói membership với ID {request.MembershipId}");
                }

                // Tạo order ID
                string orderId = $"ORDER_{DateTime.UtcNow.Ticks}";

                // Tạo payload cho PayOS
                var payload = new
                {
                    orderCode = orderId,
                    amount = membership.Price,
                    description = $"Thanh toán gói {membership.Name}",
                    returnUrl = request.ReturnUrl,
                    cancelUrl = request.CancelUrl,
                    signature = CreateSignature(orderId, membership.Price)
                };

                // Gọi API PayOS
                var response = await _httpClient.PostAsJsonAsync($"{_payOSConfig.BaseUrl}/v1/payment-requests", payload);
                response.EnsureSuccessStatusCode();

                var paymentResponse = await response.Content.ReadFromJsonAsync<PaymentResponseDTO>();
                return paymentResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo yêu cầu thanh toán");
                throw;
            }
        }

        public async Task<bool> VerifyPaymentAsync(string orderId, decimal amount, string checksum)
        {
            try
            {
                var calculatedChecksum = CreateSignature(orderId, amount);
                return calculatedChecksum == checksum;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi verify payment");
                return false;
            }
        }

        public async Task<bool> HandlePaymentWebhookAsync(PaymentWebhookDTO webhookData)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Verify webhook signature
                if (!await VerifyPaymentAsync(webhookData.OrderId, webhookData.Amount, webhookData.PaymentId))
                {
                    throw new InvalidOperationException("Invalid payment signature");
                }

                // Xử lý theo trạng thái thanh toán
                if (webhookData.Status == "SUCCESS")
                {
                    // Parse order ID để lấy thông tin user và membership
                    var orderParts = webhookData.OrderId.Split('_');
                    if (orderParts.Length != 4) // Format: ORDER_USERID_MEMBERSHIPID_TIMESTAMP
                    {
                        throw new InvalidOperationException("Invalid order ID format");
                    }

                    int userId = int.Parse(orderParts[1]);
                    int membershipId = int.Parse(orderParts[2]);

                    // Tạo membership mới cho user
                    var createDto = new CreateUserMembershipDTO
                    {
                        UserId = userId,
                        MembershipId = membershipId
                    };

                    await _userMembershipService.CreateUserMembershipAsync(createDto);
                    await transaction.CommitAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi xử lý webhook thanh toán");
                throw;
            }
        }

        private string CreateSignature(string orderId, decimal amount)
        {
            var data = $"{_payOSConfig.ClientId}{orderId}{amount}{_payOSConfig.ChecksumKey}";
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(data);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
