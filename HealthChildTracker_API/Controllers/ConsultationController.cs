using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
                var createdRequest = await _consultationService.CreateRequestAsync(userId, request);
                return CreatedAtAction(nameof(GetRequest), new { requestId = createdRequest.RequestId }, createdRequest);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo yêu cầu tư vấn");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("request/{requestId}")]
        public async Task<IActionResult> GetRequest(int requestId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var request = await _consultationService.GetRequestByIdAsync(requestId, userId);
                return Ok(request);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin yêu cầu tư vấn");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("user/requests")]
        public async Task<IActionResult> GetUserRequests()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var requests = await _consultationService.GetUserRequestsAsync(userId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách yêu cầu tư vấn của người dùng");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("doctor/requests")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorRequests()
        {
            try
            {
                var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var requests = await _consultationService.GetDoctorRequestsAsync(doctorId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách yêu cầu tư vấn của bác sĩ");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPost("request/{requestId}/response")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateResponse(
            int requestId,
            [FromBody] CreateConsultationResponseDTO response)
        {
            try
            {
                var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                response.RequestId = requestId;
                var createdResponse = await _consultationService.CreateResponseAsync(doctorId, response);
                return Ok(createdResponse);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phản hồi");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPost("request/{requestId}/question")]
        public async Task<IActionResult> AskQuestion(
            int requestId,
            [FromBody] string question)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var response = await _consultationService.AddUserQuestionAsync(requestId, userId, question);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm câu hỏi");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPost("request/{requestId}/complete")]
        public async Task<IActionResult> CompleteRequest(
            int requestId,
            [FromBody] bool isSatisfied)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var request = await _consultationService.CompleteRequestAsync(requestId, userId, isSatisfied);
                return Ok(request);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn thành yêu cầu tư vấn");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }
}