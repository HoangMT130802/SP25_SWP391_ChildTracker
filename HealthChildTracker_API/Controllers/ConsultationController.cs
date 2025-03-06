using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
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

        // tạo yêu cầu
        [HttpPost("Createrequest")]
        public async Task<IActionResult> CreateRequest(int userId, [FromBody] CreateConsultationRequestDTO request)
        {
            try
            {
                var newRequest = await _consultationService.CreateRequestAsync(userId, request);
                return Ok(newRequest);
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
                var request = await _consultationService.GetRequestByIdAsync(requestId);
                return Ok(request);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin yêu cầu tư vấn");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("user/requests")]
        //[Authorize]
        public async Task<IActionResult> GetUserRequests([FromQuery] int userId)
        {
            try
            {
                //var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
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
        //[Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetDoctorRequests([FromQuery] int doctorId)
        {
            try
            {
                //var doctorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var requests = await _consultationService.GetDoctorRequestsAsync(doctorId);
                return Ok(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách yêu cầu tư vấn của bác sĩ");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPut("response/{responseId}")]
        //[Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateResponse(
            int responseId,
            [FromBody] UpdateResponseDTO updateRequest)
        {
            try
            {
                var response = await _consultationService.UpdateResponseAsync(responseId, updateRequest);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phản hồi");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpPost("assign/{requestId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignDoctor(
            int requestId,
            [FromBody] int doctorId)
        {
            try
            {
                var request = await _consultationService.AssignDoctorAsync(requestId, doctorId);
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
                _logger.LogError(ex, "Lỗi khi phân công bác sĩ");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        [HttpGet("doctor/workload")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDoctorWorkload()
        {
            try
            {
                var workload = await _consultationService.GetDoctorWorkloadAsync();
                return Ok(workload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin khối lượng công việc của bác sĩ");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }
}