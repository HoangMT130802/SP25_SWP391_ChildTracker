using BusinessLogic.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> CreatePaymentAsync(PaymentRequestDTO request);
        Task<bool> VerifyPaymentAsync(string orderId, decimal amount, string checksum);
        Task<bool> HandlePaymentWebhookAsync(PaymentWebhookDTO webhookData); 
    }
}
