using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.Services.Implementations;
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
            try
            {
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation($"User claims: {string.Join(", ", User?.Claims?.Select(c => $"{c.Type}: {c.Value}"))}");
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("User ID claim not found");
                    return null;
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogWarning($"Failed to parse user ID: {userIdClaim}");
                    return null;
                }

                _logger.LogInformation($"Successfully retrieved user ID: {userId}");
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
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
        [HttpGet("GetAllrequest")]
        public async Task<IActionResult> GetAllConsulationRequests()
        {
            try
            {
                var consultations = await _consultationService.GetAllConsulationRequest();
                return Ok(consultations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all requests");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        [HttpGet("request/{requestId}")]
        
        public async Task<IActionResult> GetRequest(int requestId)
        {
            try
            {
                // Log request headers
                _logger.LogInformation($"Authorization header: {Request.Headers["Authorization"]}");
                _logger.LogInformation($"Request for consultation ID: {requestId}");

                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    _logger.LogWarning("Unauthorized access attempt - no valid user ID");
                    return Unauthorized(new { message = "Không thể xác thực người dùng. Vui lòng đăng nhập lại." });
                }

                var request = await _consultationService.GetRequestByIdAsync(requestId, userId.Value);
                return Ok(request);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Consultation request {requestId} not found");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, $"Unauthorized access to consultation {requestId}");
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving consultation request {requestId}");
                return StatusCode(500, new { message = "Lỗi server khi xử lý yêu cầu" });
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
            [FromBody] DoctorResponseDTO responseDto)
        {
            try
            {
                var doctorId = GetCurrentUserId();
                if (!doctorId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực bác sĩ" });
                }

                var response = await _consultationService.AddResponseAsync(
                    requestId,
                    doctorId.Value,
                    new AskQuestionDTO { Question = responseDto.Answer, Attachments = responseDto.Attachments },
                    isFromDoctor: true);
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
                _logger.LogError(ex, "Lỗi khi tạo phản hồi");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
        [HttpPost("request/{requestId}/question")]
        public async Task<IActionResult> AskQuestion(
            int requestId,
            [FromBody] AskQuestionDTO questionDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var response = await _consultationService.AddResponseAsync(
                    requestId,
                    userId.Value,
                    questionDto);
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

        [HttpPost("request/{requestId}/response/{questionId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AnswerQuestion(
            int requestId,
            int questionId,
            [FromBody] DoctorResponseDTO answerDto)
        {
            try
            {
                var doctorId = GetCurrentUserId();
                if (!doctorId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực bác sĩ" });
                }

                var response = await _consultationService.AddResponseAsync(
                    requestId,
                    doctorId.Value,
                    new AskQuestionDTO { Question = answerDto.Answer, Attachments = answerDto.Attachments },
                    parentResponseId: questionId,
                    isFromDoctor: true);
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
                _logger.LogError(ex, "Lỗi khi trả lời câu hỏi");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPost("request/{requestId}/question/{responseId}")]
        public async Task<IActionResult> AskFollowUpQuestion(
            int requestId,
            int responseId,
            [FromBody] AskQuestionDTO questionDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var response = await _consultationService.AddResponseAsync(
                    requestId,
                    userId.Value,
                    questionDto,
                    parentResponseId: responseId);
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

                var request = await _consultationService.UpdateRequestStatusAsync(
                    requestId,
                    userId.Value,
                    "complete",
                    isSatisfied: isSatisfied);
                return Ok(new { 
                    message = isSatisfied ? "Cảm ơn bạn đã sử dụng dịch vụ" : "Chúng tôi rất tiếc vì chưa đáp ứng được yêu cầu của bạn",
                    request = request 
                });
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