
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
        public async Task<IActionResult> TotalUsersCreateByDate([FromQuery] string? date)
        {
            var result = await _adminService.TotalUsersCreateByDateAsync(date);
            return Ok(result);
        }

        [HttpGet("total-users-created-month")]
        public async Task<IActionResult> TotalUsersCreateByMonth([FromQuery] string? date)
        {
            var result = await _adminService.TotalUsersCreateByMonthAsync(date);
            return Ok(result);
        }

        [HttpGet("total-active-and-Blocked")]
        public async Task<IActionResult> GetUserStatusStatisticsAsync()
        {
            var result = await _adminService.GetUserStatusStatisticsAsync();
            return Ok(result);
        }
    }
}
