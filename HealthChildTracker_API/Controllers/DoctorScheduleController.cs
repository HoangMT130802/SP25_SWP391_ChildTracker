using BusinessLogic.DTOs.Doctor_Schedule;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorScheduleController : ControllerBase
    {
        private readonly IDoctorScheduleService _scheduleService;
        private readonly ILogger<DoctorScheduleController> _logger;

        public DoctorScheduleController(IDoctorScheduleService scheduleService, ILogger<DoctorScheduleController> logger)
        {
            _scheduleService = scheduleService;
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

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateDoctorScheduleDTO scheduleDTO)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực bác sĩ" });
                }

                // Đảm bảo bác sĩ chỉ tạo lịch cho chính mình
                if (currentUserId.Value != scheduleDTO.DoctorId)
                {
                    return Unauthorized(new { message = "Bác sĩ chỉ được tạo lịch cho chính mình" });
                }

                var createdSchedule = await _scheduleService.CreateScheduleAsync(scheduleDTO);
                return Ok(createdSchedule);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch làm việc");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{scheduleId}/slots")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableSlots(int scheduleId)
        {
            try
            {
                var slots = await _scheduleService.GetAvailableSlotsAsync(scheduleId);
                return Ok(slots);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy các slot cho lịch {scheduleId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("slots/default")]
        [AllowAnonymous]
        public IActionResult GetDefaultTimeSlots()
        {
            try
            {
                var slots = _scheduleService.GetDefaultTimeSlots();
                return Ok(slots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách slot mặc định");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("doctor/{doctorId}/week")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorSchedulesByWeek(
            int doctorId,
            [FromQuery] string weekStart)
        {
            try
            {
                if (!DateOnly.TryParseExact(weekStart, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly startDate))
                {
                    return BadRequest(new { message = "Ngày bắt đầu tuần không đúng định dạng (yyyy-MM-dd)" });
                }

                while (startDate.DayOfWeek != DayOfWeek.Monday)
                {
                    startDate = startDate.AddDays(-1);
                }

                var schedules = await _scheduleService.GetDoctorSchedulesByWeekAsync(doctorId, startDate);
                return Ok(new
                {
                    doctorId = doctorId,
                    weekStart = startDate.ToString("yyyy-MM-dd"),
                    weekEnd = startDate.AddDays(4).ToString("yyyy-MM-dd"),
                    schedules = schedules
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch làm việc của bác sĩ {doctorId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
