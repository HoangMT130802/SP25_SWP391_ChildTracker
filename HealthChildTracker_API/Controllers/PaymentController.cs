using BusinessLogic.DTOs.Payment;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(PaymentResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PaymentResponseDTO>> CreatePayment([FromBody] PaymentRequestDTO request)
        {
            try
            {
                var response = await _paymentService.CreatePaymentAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo yêu cầu thanh toán");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] PaymentWebhookDTO webhookData)
        {
            try
            {
                var result = await _paymentService.HandlePaymentWebhookAsync(webhookData);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý webhook");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyPayment(
            [FromQuery] string orderId,
            [FromQuery] decimal amount,
            [FromQuery] string checksum)
        {
            try
            {
                var isValid = await _paymentService.VerifyPaymentAsync(orderId, amount, checksum);
                return Ok(new { isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi verify payment");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
