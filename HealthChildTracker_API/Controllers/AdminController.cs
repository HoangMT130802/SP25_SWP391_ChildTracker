using BusinessLogic.DTOs.Children;
using BusinessLogic.Services.Implementations;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            this._adminService = adminService;
        }

        [HttpGet("statisticsRole")]
        public async Task<IActionResult> GetUserStatisticsRole()
        {
            var statistics = await _adminService.GetUserStatisticsRoleAsync();
            int totalUsers = statistics.Values.Sum();

            return Ok(new
            {
                total_users = totalUsers,
                roles = statistics
            });
        }

        [HttpGet("total-users-created-day")]
        public async Task<IActionResult> GetUsersByDate([FromQuery] string? date)
        {
            var result = await _adminService.GetUsersCreatedOnDateAsync(date);
            return Ok(result);
        }
    }
}
