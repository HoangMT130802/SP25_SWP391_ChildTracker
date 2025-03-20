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

        public PaymentController(
            IPaymentService paymentService,
            ILogger<PaymentController> logger)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("create")]
        public async Task<ActionResult<PaymentResponseDTO>> CreatePayment([FromBody] PaymentRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Request không được để trống" });
                }

                _logger.LogInformation("Bắt đầu tạo payment cho user {UserId}", request.UserId);
                var result = await _paymentService.CreatePaymentAsync(request);

                if (result == null)
                {
                    return BadRequest(new { success = false, message = "Không thể tạo payment" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo payment cho user {UserId}", request?.UserId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("check-status/{orderId}")]
        public async Task<ActionResult<PaymentStatusDTO>> CheckPaymentStatus(string orderId)
        {
            try
            {
                if (string.IsNullOrEmpty(orderId))
                {
                    return BadRequest(new { success = false, message = "OrderId không được để trống" });
                }

                _logger.LogInformation("Kiểm tra trạng thái payment cho order {OrderId}", orderId);
                var result = await _paymentService.CheckPaymentStatusAsync(orderId);

                if (result == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin payment" });
                }

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra trạng thái payment cho order {OrderId}", orderId);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
