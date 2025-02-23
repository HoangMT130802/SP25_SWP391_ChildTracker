
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorProfile>>> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            return Ok(doctors);
        }


        [HttpGet("{doctorId}")]
        public async Task<ActionResult<DoctorProfile>> GetDoctorById(int doctorId)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(doctorId);
            if (doctor == null)
            {
                return NotFound(new { message = "Doctor not found" });
            }
            return Ok(doctor);
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
