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
    [Authorize]
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
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
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
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
                    return NotFound(new { message = $"Không tìm thấy membership với ID {id}" });
                }
                return Ok(membership);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Không tìm thấy membership với ID {Id}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin user membership {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }

        [HttpGet("user/{userId}/active")]
        [Authorize]
        [ProducesResponseType(typeof(UserMembershipDTO), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserMembershipDTO>> GetActiveUserMembership(int userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                // Kiểm tra quyền truy cập
                if (!User.IsInRole("Admin") && currentUserId.Value != userId)
                {
                    return Forbid();
                }

                var membership = await _userMembershipService.GetActiveUserMembershipAsync(userId);
                if (membership == null)
                {
                    return NotFound(new { message = "Không tìm thấy gói membership đang hoạt động" });
                }

                return Ok(membership);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy active membership của user {UserId}", userId);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
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
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái membership {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
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
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var membership = await _userMembershipService.GetUserMembershipByIdAsync(id);
                if (membership == null)
                {
                    return NotFound(new { message = "Không tìm thấy membership" });
                }

                // Kiểm tra quyền truy cập
                if (!User.IsInRole("Admin") && currentUserId.Value != membership.UserId)
                {
                    return Forbid();
                }

                var result = await _userMembershipService.DecrementConsultationCountAsync(id);
                return Ok(new { success = result });
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
                _logger.LogError(ex, "Lỗi khi giảm số lượt tư vấn của membership {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
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
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "Không thể xác thực người dùng" });
                }

                var membership = await _userMembershipService.GetUserMembershipByIdAsync(id);
                if (membership == null)
                {
                    return NotFound(new { message = "Không tìm thấy membership" });
                }

                // Kiểm tra quyền truy cập
                if (!User.IsInRole("Admin") && currentUserId.Value != membership.UserId)
                {
                    return Forbid();
                }

                var result = await _userMembershipService.RenewMembershipAsync(id);
                return Ok(new { success = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gia hạn membership {Id}", id);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }
    }
}
