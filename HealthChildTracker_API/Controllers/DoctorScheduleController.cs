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
    
    public class DoctorScheduleController : ControllerBase
    {
        private readonly IDoctorScheduleService _scheduleService;
        private readonly ILogger<DoctorScheduleController> _logger;

        public DoctorScheduleController(IDoctorScheduleService scheduleService, ILogger<DoctorScheduleController> logger)
        {
            _scheduleService = scheduleService;
            _logger = logger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateDoctorScheduleDTO scheduleDTO)
        {
            try
            {
                // Kiểm tra xác thực nếu có
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRoleClaim = User.FindFirst(ClaimTypes.Role);

                // Nếu user đã đăng nhập và là bác sĩ
                if (userIdClaim != null && userRoleClaim?.Value == "Doctor")
                {
                    var userId = int.Parse(userIdClaim.Value);
                    if (userId != scheduleDTO.DoctorId)
                    {
                        return Forbid("Bác sĩ chỉ được tạo lịch cho chính mình");
                    }
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
                // Parse weekStart từ string sang DateOnly
                if (!DateOnly.TryParseExact(weekStart, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly startDate))
                {
                    return BadRequest(new { message = "Ngày bắt đầu tuần không đúng định dạng (yyyy-MM-dd)" });
                }

                // Đảm bảo ngày bắt đầu là thứ 2
                while (startDate.DayOfWeek != DayOfWeek.Monday)
                {
                    startDate = startDate.AddDays(-1);
                }

                var schedules = await _scheduleService.GetDoctorSchedulesByWeekAsync(doctorId, startDate);
                return Ok(new
                {
                    doctorId = doctorId,
                    weekStart = startDate.ToString("yyyy-MM-dd"),
                    weekEnd = startDate.AddDays(4).ToString("yyyy-MM-dd"), // Thứ 6
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
