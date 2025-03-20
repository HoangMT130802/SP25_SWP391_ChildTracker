using BusinessLogic.DTOs.Payment;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        [HttpPost("create")]
        public async Task<ActionResult<PaymentResponseDTO>> CreatePayment([FromBody] PaymentRequestDTO request)
        {
            try
            {
                var result = await _paymentService.CreatePaymentAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("check-status/{orderId}")]
        public async Task<ActionResult<PaymentStatusDTO>> CheckPaymentStatus(string orderId)
        {
            try
            {
                var result = await _paymentService.CheckPaymentStatusAsync(orderId);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
