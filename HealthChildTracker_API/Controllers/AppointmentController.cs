using BusinessLogic.DTOs.Appointment;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   /* [Authorize]*/
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        [HttpGet]
        /*[Authorize(Roles = "Admin")]*/
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all appointments");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAppointments(int userId)
        {
            try
            {
              
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != userId && userRole != "Admin")
                {
                    return Forbid("You don't have permission to view these appointments");
                }

                var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for user {userId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorAppointments(int doctorId)
        {
            try
            {
               
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != doctorId && userRole != "Admin")
                {
                    return Forbid("You don't have permission to view these appointments");
                }

                var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for doctor {doctorId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("child/{childId}")]
        public async Task<IActionResult> GetChildAppointments(int childId)
        {
            try
            {
               

                var appointments = await _appointmentService.GetChildAppointmentsAsync(childId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for child {childId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{appointmentId}")]
        public async Task<IActionResult> GetAppointmentById(int appointmentId)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

               
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != appointment.UserId && currentUserId != appointment.DoctorId && userRole != "Admin")
                {
                    return Forbid("You don't have permission to view this appointment");
                }

                return Ok(appointment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointment {appointmentId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO appointmentDTO)
        {
            try
            {
                
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (currentUserId != appointmentDTO.UserId)
                {
                    return Forbid("You can only create appointments for yourself");
                }

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
                _logger.LogError(ex, "Error creating appointment");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{appointmentId}")]
        public async Task<IActionResult> UpdateAppointment(int appointmentId, [FromBody] UpdateAppointmentDTO appointmentDTO)
        {
            try
            {
                
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

                if (currentUserId != appointment.DoctorId && userRole != "Admin")
                {
                    return Forbid("Only doctors or admins can update appointments");
                }

                var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(appointmentId, appointmentDTO);
                return Ok(updatedAppointment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating appointment {appointmentId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{appointmentId}/cancel")]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var result = await _appointmentService.CancelAppointmentAsync(appointmentId, currentUserId);
                return Ok(new { success = result, message = "Appointment cancelled successfully" });
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
                _logger.LogError(ex, $"Error cancelling appointment {appointmentId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("user/{userId}/daterange")]
        public async Task<IActionResult> GetAppointmentsByDateRange(
            int userId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            try
            {
              
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != userId && userRole != "Admin")
                {
                    return Forbid("You don't have permission to view these appointments");
                }

                var appointments = await _appointmentService.GetAppointmentsByDateRangeAsync(userId, startDate, endDate);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for user {userId} between {startDate} and {endDate}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("doctor/{doctorId}/date/{date}")]
        public async Task<IActionResult> GetDoctorAppointmentsByDate(int doctorId, DateOnly date)
        {
            try
            {
                
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != doctorId && userRole != "Admin")
                {
                    return Forbid("You don't have permission to view these appointments");
                }

                var appointments = await _appointmentService.GetDoctorAppointmentsByDateAsync(doctorId, date);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for doctor {doctorId} on {date}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
