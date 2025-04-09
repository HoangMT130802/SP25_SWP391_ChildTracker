using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Appointment;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthChildTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
        {
            _appointmentService = appointmentService;
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

        [HttpGet("GetAppoinmentByUserId/{userId}")]
        public async Task<IActionResult> GetUserAppointments(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                // Chỉ cho phép xem lịch hẹn của chính mình
                if (currentUserId.Value != userId)
                {
                    return Forbid();
                }

                var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách lịch hẹn của người dùng {userId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách lịch hẹn" });
            }
        }

        [HttpGet("GetAppoinmentByDoctor/{doctorId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> GetDoctorAppointments(int doctorId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực bác sĩ" });
                }

                // Chỉ cho phép bác sĩ xem lịch hẹn của chính mình
                if (currentUserId.Value != doctorId)
                {
                    return Forbid();
                }

                var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách lịch hẹn của bác sĩ {doctorId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách lịch hẹn" });
            }
        }

        [HttpGet("GetAppoinmentBy/{appointmentId}")]
        public async Task<IActionResult> GetAppointmentById(int appointmentId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return NotFound(new { message = "Không tìm thấy lịch hẹn" });
                }

                // Kiểm tra quyền truy cập
                if (appointment.UserId != currentUserId && appointment.DoctorId != currentUserId)
                {
                    return Forbid();
                }

                return Ok(appointment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin lịch hẹn {appointmentId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin lịch hẹn" });
            }
        }

        [HttpPost("User create the appoinment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO appointmentDTO)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                if (appointmentDTO == null)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });
                }

                // Đảm bảo người dùng chỉ tạo lịch hẹn cho chính mình
                appointmentDTO.UserId = currentUserId.Value;

                var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointmentDTO);
                return CreatedAtAction(nameof(GetAppointmentById), new { appointmentId = createdAppointment.AppointmentId }, createdAppointment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch hẹn");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo lịch hẹn" });
            }
        }

        [HttpPost("{appointmentId}/User cancel the appoinment")]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var result = await _appointmentService.CancelAppointmentAsync(appointmentId, currentUserId.Value);
                return Ok(new { success = result, message = "Hủy lịch hẹn thành công" });
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
                _logger.LogError(ex, $"Lỗi khi hủy lịch hẹn {appointmentId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi hủy lịch hẹn" });
            }
        }

        [HttpPost("{appointmentId}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CompleteAppointment(int appointmentId, [FromBody] CompleteAppointmentDTO completeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác định thông tin bác sĩ" });
                }

                var completedAppointment = await _appointmentService.CompleteAppointmentAsync(
                    appointmentId,
                    completeDto.Note,
                    currentUserId.Value
                );

                return Ok(completedAppointment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Không tìm thấy cuộc hẹn {appointmentId}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, $"Bác sĩ {User.FindFirstValue(ClaimTypes.NameIdentifier)} không có quyền hoàn thành cuộc hẹn {appointmentId}");
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Lỗi khi hoàn thành cuộc hẹn {appointmentId}: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi hoàn thành cuộc hẹn {appointmentId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi hoàn thành cuộc hẹn" });
            }
        }
    }
}