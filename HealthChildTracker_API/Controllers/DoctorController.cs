
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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


        // tìm kiếm theo chuyên môn 
        [HttpGet("SearchDoctorBySpecialization")]
        public async Task<ActionResult> GetDoctorBySpecialization(string search)
        {
            try
            {
                var doctors = await _doctorService.SearchSpecialization(search);
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> CreateDoctor([FromBody] CreateDoctorDTO doctorDto)
        {
            if (doctorDto == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            await _doctorService.CreateDoctorAsync(doctorDto);
            return CreatedAtAction(nameof(GetDoctorById), new { doctorId = doctorDto.UserId }, doctorDto);
        }


        [HttpPut("{doctorId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateDoctor(int doctorId, [FromBody] UpdateDoctorDTO doctorDto)
        {
            if (doctorDto == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            await _doctorService.UpdateDoctorAsync(doctorId, doctorDto);
            return NoContent();
        }

        [HttpDelete("{doctorId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDoctor(int doctorId)
        {
            await _doctorService.DeleteDoctorAsync(doctorId);
            return NoContent();
        }
    }
}
