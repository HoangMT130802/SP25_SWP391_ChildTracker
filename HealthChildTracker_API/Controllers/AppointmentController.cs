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
   
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        [HttpGet("Get information about the appoinment by/{userId}")]
        public async Task<IActionResult> GetUserAppointments(int userId)
        {
            try
            {
                // Tạm thời bỏ kiểm tra quyền để test
                var appointments = await _appointmentService.GetUserAppointmentsAsync(userId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách lịch hẹn của người dùng {userId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách lịch hẹn" });
            }
        }

        [HttpGet("Get information about the appoinment by/{doctorId}")]
        public async Task<IActionResult> GetDoctorAppointments(int doctorId)
        {
            try
            {
                // Tạm thời bỏ kiểm tra quyền để test
                var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách lịch hẹn của bác sĩ {doctorId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách lịch hẹn" });
            }
        }

        [HttpGet("Get information about the appoinment by/{appointmentId}")]
        public async Task<IActionResult> GetAppointmentById(int appointmentId)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    return NotFound(new { message = "Không tìm thấy lịch hẹn" });
                }

                // Tạm thời bỏ kiểm tra quyền để test
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
                if (appointmentDTO == null)
                {
                    return BadRequest(new { message = "Dữ liệu không hợp lệ" });
                }

                // Kiểm tra dữ liệu đầu vào
                if (appointmentDTO.ScheduleId <= 0)
                {
                    return BadRequest(new { message = "ID lịch làm việc không hợp lệ" });
                }

                if (appointmentDTO.UserId <= 0)
                {
                    return BadRequest(new { message = "ID người dùng không hợp lệ" });
                }

                if (appointmentDTO.ChildId <= 0)
                {
                    return BadRequest(new { message = "ID trẻ không hợp lệ" });
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
                _logger.LogError(ex, "Lỗi khi tạo lịch hẹn");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo lịch hẹn" });
            }
        }

        [HttpPost("{appointmentId}/User cancel the appoinment")]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                // Tạm thời bỏ kiểm tra quyền để test
                var result = await _appointmentService.CancelAppointmentAsync(appointmentId, 0);
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

      

       

        [HttpPost("{appointmentId}/Change status to completed")]
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            try
            {
                var result = await _appointmentService.CompleteAppointmentAsync(appointmentId);
                return Ok(new { success = true, message = "Hoàn thành cuộc hẹn thành công", appointment = result });
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
                _logger.LogError(ex, $"Lỗi khi hoàn thành lịch hẹn {appointmentId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi hoàn thành cuộc hẹn" });
            }
        }
    }
} 