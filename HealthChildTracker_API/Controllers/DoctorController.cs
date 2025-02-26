using BusinessLogic.DTOs.Doctor;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HealthChildTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous] // Cho phép xem danh sách bác sĩ mà không cần đăng nhập
        public async Task<IActionResult> GetAllDoctors()
        {
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all doctors");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{doctorId}")]
        [AllowAnonymous] // Cho phép xem thông tin bác sĩ mà không cần đăng nhập
        public async Task<IActionResult> GetDoctorById(int doctorId)
        {
            try
            {
                var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
                return Ok(doctor);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting doctor {doctorId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được tạo bác sĩ
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDTO doctorDTO)
        {
            try
            {
                var doctor = await _doctorService.CreateDoctorAsync(doctorDTO);
                return CreatedAtAction(nameof(GetDoctorById), new { doctorId = doctor.UserId }, doctor);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating doctor");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{doctorId}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được cập nhật thông tin bác sĩ
        public async Task<IActionResult> UpdateDoctor(int doctorId, [FromBody] UpdateDoctorDTO doctorDTO)
        {
            try
            {
                var doctor = await _doctorService.UpdateDoctorAsync(doctorId, doctorDTO);
                return Ok(doctor);
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
                _logger.LogError(ex, $"Error updating doctor {doctorId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{doctorId}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xóa bác sĩ
        public async Task<IActionResult> DeleteDoctor(int doctorId)
        {
            try
            {
                var result = await _doctorService.DeleteDoctorAsync(doctorId);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting doctor {doctorId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
