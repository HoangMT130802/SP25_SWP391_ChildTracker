using BusinessLogic.DTOs.UserMembership;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserMembershipController : ControllerBase
    {
        private readonly IUserMembershipService _userMembershipService;
        private readonly ILogger<UserMembershipController> _logger;

        public UserMembershipController(
            IUserMembershipService userMembershipService,
            ILogger<UserMembershipController> logger)
        {
            _userMembershipService = userMembershipService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<UserMembershipDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserMembershipDTO>>> GetAllUserMemberships()
        {
            try
            {
                var memberships = await _userMembershipService.GetAllUserMembershipsAsync();
                return Ok(memberships);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách user membership");
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserMembershipDTO>> GetUserMembershipById(int id)
        {
            try
            {
                var membership = await _userMembershipService.GetUserMembershipByIdAsync(id);
                if (membership == null)
                {
                    return NotFound($"Không tìm thấy membership với ID {id}");
                }
                return Ok(membership);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy membership với ID {Id}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user membership {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }
        public async Task<ActionResult<UserMembershipDTO>> GetActiveUserMembership(int userId)
        {
            try
            {
                // Log thông tin user từ token để debug
                _logger.LogInformation("Claims trong token: {Claims}",
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

                // Lấy UserId từ claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogWarning("Không tìm thấy claim NameIdentifier trong token");
                    return Unauthorized("Không tìm thấy thông tin người dùng trong token");
                }

                if (!int.TryParse(userIdClaim.Value, out int currentUserId))
                {
                    _logger.LogWarning("Giá trị UserId trong token không hợp lệ: {UserId}", userIdClaim.Value);
                    return Unauthorized("Thông tin người dùng không hợp lệ");
                }

                // Kiểm tra quyền truy cập
                if (!User.IsInRole("Admin") && currentUserId != userId)
                {
                    _logger.LogWarning("Người dùng {CurrentUserId} không có quyền truy cập thông tin của user {UserId}",
                        currentUserId, userId);
                    return Forbid();
                }

                var membership = await _userMembershipService.GetActiveUserMembershipAsync(userId);
                if (membership == null)
                {
                    _logger.LogInformation("Không tìm thấy active membership cho user {UserId}", userId);
                    return NotFound("Không tìm thấy gói membership đang hoạt động");
                }

                return Ok(membership);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy active membership của user {UserId}", userId);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMembershipStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _userMembershipService.UpdateMembershipStatusAsync(id, status);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái membership {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }

        [HttpPost("{id}/decrement-consultation")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DecrementConsultationCount(int id)
        {
            try
            {
                var membership = await _userMembershipService.GetUserMembershipByIdAsync(id);

                // Kiểm tra quyền truy cập
                if (!User.IsInRole("Admin") && int.Parse(User.FindFirst("UserId").Value) != membership.UserId)
                {
                    return Forbid();
                }

                var result = await _userMembershipService.DecrementConsultationCountAsync(id);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi giảm số lượt tư vấn của membership {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }

        [HttpPost("{id}/renew")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RenewMembership(int id)
        {
            try
            {
                var membership = await _userMembershipService.GetUserMembershipByIdAsync(id);

                // Kiểm tra quyền truy cập
                if (!User.IsInRole("Admin") && int.Parse(User.FindFirst("UserId").Value) != membership.UserId)
                {
                    return Forbid();
                }

                var result = await _userMembershipService.RenewMembershipAsync(id);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gia hạn membership {Id}", id);
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu");
            }
        }
    }
}
