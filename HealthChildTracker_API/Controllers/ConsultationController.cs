using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
   /* [ApiController]
    [Route("api/[controller]")]
    public class ConsultationController : ControllerBase
    {
        private readonly IConsultationService _consultationService;
        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(
            IConsultationService consultationService,
            ILogger<ConsultationController> logger)
        {
            _consultationService = consultationService;
            _logger = logger;
        }

        [HttpPost("request")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateConsultationRequestDTO request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _consultationService.CreateRequestAsync(userId, request);
                return CreatedAtAction(nameof(GetRequest), new { id = result.RequestId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo yêu cầu tư vấn");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPost("response")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateResponse([FromBody] CreateConsultationResponseDTO response)
        {
            try
            {
                var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var result = await _consultationService.CreateResponseAsync(doctorId, response);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phản hồi tư vấn");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPost("close/{requestId}")]
        public async Task<IActionResult> CloseRequest(
            int requestId,
            [FromBody] string reason)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role).Value;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                var result = await _consultationService.CloseRequestAsync(requestId, reason, $"{userRole}:{userId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đóng yêu cầu tư vấn");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }*/
}