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

        [HttpGet("Get all doctors")]
       
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

        [HttpGet("{userId}/get Doctor by userId")]
        
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

        [HttpPost("{DoctorId}/Create new doctor")]
       
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

        [HttpPut("{userId}/update doctor by userId")]
        
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

        [HttpPut("{doctorId}/Change verification")]
        
        public async Task<IActionResult> ToggleVerification(int doctorId)
        {
            try
            {
                var isVerified = await _doctorService.ToggleDoctorVerification(doctorId);
                var message = isVerified ? "Xác thực bác sĩ thành công" : "Hủy xác thực bác sĩ thành công";
                return Ok(new { success = true, isVerified = isVerified, message = message });
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
                _logger.LogError(ex, $"Lỗi khi thay đổi trạng thái xác thực bác sĩ {doctorId}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi trong quá trình xử lý" });
            }
        }
    }
}
