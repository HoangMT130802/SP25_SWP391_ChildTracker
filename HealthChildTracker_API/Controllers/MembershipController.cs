using BusinessLogic.DTOs.UserMembership;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthChildTrackerAPI.Controllers
{
    [Route("api/memberships")]
    [ApiController]
    public class MembershipController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public MembershipController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        // API lấy danh sách gói Membership
        [HttpGet("list")]
        public IActionResult GetMemberships()
        {
            var memberships = _membershipService.GetMembershipPlans();
            return Ok(memberships);
        }

        // đăng ký Membership chưa thanh toán
        [HttpPost("register")]
        public async Task<IActionResult> RegisterMembership([FromBody] CreateUserMemebershipDTO userMembershipDto)
        {
            if (userMembershipDto == null)
            {
                return BadRequest("Invalid data.");
            }

            bool isRegistered = await _membershipService.RegisterMembership(userMembershipDto);
            if (!isRegistered)
            {
                return BadRequest("Membership ID không hợp lệ hoặc đã tồn tại.");
            }

            return Ok("Đăng ký Membership thành công. Vui lòng thanh toán để kích hoạt.");
        }

        // Lấy danh sách tất cả UserMemberships
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserMembershipDto>>> GetAllUserMemberships()
        {
            try
            {
                var result = await _membershipService.ShowAllUserMemberships();
                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest("Không thể lấy danh sách UserMemberships.");
            }
        }

        [HttpPut("toggle-status/{userMembershipId}")]
        public async Task<IActionResult> MembershipStatus(int userMembershipId,[FromBody] bool newStatus, int userId)
        {
            try
            {
                await _membershipService.UserMembershipStatus(userMembershipId, newStatus, userId);

                return Ok(new { message = "Trạng thái đã được cập nhật." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Lỗi khi cập nhật trạng thái." });
            }
        }

        // nâng cấp gói
        [HttpPut("upgrade/{userMembershipId}")]
        public async Task<IActionResult> UpgradeMembership(int userMembershipId)
        {
            try
            {
                var success = await _membershipService.UpgradeMembership(userMembershipId);

                if (!success) return BadRequest("Nâng cấp không thành công.");
                return Ok("Nâng cấp Membership thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
