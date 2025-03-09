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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }

        [HttpPost("request")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateConsultationRequestDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var createdRequest = await _consultationService.CreateRequestAsync(userId.Value, request);
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
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var request = await _consultationService.GetRequestByIdAsync(requestId, userId.Value);
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
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var requests = await _consultationService.GetUserRequestsAsync(userId.Value);
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
                var doctorId = GetCurrentUserId();
                if (!doctorId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực bác sĩ" });
                }

                var requests = await _consultationService.GetDoctorRequestsAsync(doctorId.Value);
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
                var doctorId = GetCurrentUserId();
                if (!doctorId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực bác sĩ" });
                }

                response.RequestId = requestId;
                var createdResponse = await _consultationService.CreateResponseAsync(doctorId.Value, response);
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
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var response = await _consultationService.AddUserQuestionAsync(requestId, userId.Value, question);
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
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var request = await _consultationService.CompleteRequestAsync(requestId, userId.Value, isSatisfied);
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