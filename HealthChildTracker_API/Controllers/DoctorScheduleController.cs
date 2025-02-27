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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSchedules()
        {
            try
            {
                var schedules = await _scheduleService.GetAllSchedulesAsync();
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all doctor schedules");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorSchedules(int doctorId)
        {
            try
            {
                var schedules = await _scheduleService.GetDoctorSchedulesAsync(doctorId);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedules for doctor {doctorId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("doctor/{doctorId}/daterange")]
        public async Task<IActionResult> GetDoctorSchedulesByDateRange(
            int doctorId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            try
            {
                var schedules = await _scheduleService.GetDoctorSchedulesByDateRangeAsync(doctorId, startDate, endDate);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedules for doctor {doctorId} between {startDate} and {endDate}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{scheduleId}")]
        public async Task<IActionResult> GetScheduleById(int scheduleId)
        {
            try
            {
                var schedule = await _scheduleService.GetScheduleByIdAsync(scheduleId);
                return Ok(schedule);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedule {scheduleId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateDoctorScheduleDTO scheduleDTO)
        {
            try
            {
               
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (userRole == "Doctor" && userId != scheduleDTO.DoctorId)
                {
                    return Forbid("Doctors can only create schedules for themselves");
                }

                var createdSchedule = await _scheduleService.CreateScheduleAsync(scheduleDTO);
                return CreatedAtAction(nameof(GetScheduleById), new { scheduleId = createdSchedule.ScheduleId }, createdSchedule);
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
                _logger.LogError(ex, "Error creating doctor schedule");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{scheduleId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateSchedule(int scheduleId, [FromBody] UpdateDoctorScheduleDTO scheduleDTO)
        {
            try
            {
                
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var currentSchedule = await _scheduleService.GetScheduleByIdAsync(scheduleId);
                if (userRole == "Doctor" && userId != currentSchedule.DoctorId)
                {
                    return Forbid("Doctors can only update their own schedules");
                }

                var updatedSchedule = await _scheduleService.UpdateScheduleAsync(scheduleId, scheduleDTO);
                return Ok(updatedSchedule);
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
                _logger.LogError(ex, $"Error updating schedule {scheduleId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{scheduleId}")]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeleteSchedule(int scheduleId)
        {
            try
            {
                
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var currentSchedule = await _scheduleService.GetScheduleByIdAsync(scheduleId);
                if (userRole == "Doctor" && userId != currentSchedule.DoctorId)
                {
                    return Forbid("Doctors can only delete their own schedules");
                }

                var result = await _scheduleService.DeleteScheduleAsync(scheduleId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting schedule {scheduleId}");
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
                _logger.LogError(ex, $"Error getting available slots for schedule {scheduleId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{scheduleId}/slots/{slotTime}/available")]
        public async Task<IActionResult> IsSlotAvailable(int scheduleId, TimeOnly slotTime)
        {
            try
            {
                var isAvailable = await _scheduleService.IsSlotAvailableAsync(scheduleId, slotTime);
                return Ok(new { isAvailable });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if slot {slotTime} is available for schedule {scheduleId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
