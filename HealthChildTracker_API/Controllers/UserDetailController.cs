using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.Services.Implementations;

namespace HealthChildTrackerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserDetailController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserDetailController(IUserService userService)
        {
            _userService = userService;
        }

        // Lấy Thông tin chi tiết User
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            var userDetailsDTO = new UserDetailsDTO { UserId = id };  // Gói id vào DTO

            try
            {
                var userDetails = await _userService.UserDetail(userDetailsDTO);
                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message); 
            }
        }

        // Cập nhật thông tin User
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDTO request)
        {
            try
            {
                var updatedUser = await _userService.UserUpdate(id, request);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

