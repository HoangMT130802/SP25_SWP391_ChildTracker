using System.Security.Claims;
using BusinessLogic.DTOs.Children;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChildrenController : ControllerBase
    {
        private readonly IChildService _childService;
        private readonly ILogger<ChildrenController> _logger;

        public ChildrenController(IChildService childService, ILogger<ChildrenController> logger)
        {
            _childService = childService ?? throw new ArgumentNullException(nameof(childService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateUserAccess(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return false;
            }
            return currentUserId == userId || User.IsInRole("Admin");
        }

        /// Lấy all trẻ của user 
        [HttpGet("{userId}/Get children by userId")]
        public async Task<IActionResult> GetAllChildrenByUserId(int userId)
        {
            try
            {
                if (!ValidateUserAccess(userId))
                {
                    return Forbid("Bạn không có quyền xem thông tin này");
                }

                var children = await _childService.GetAllChildrenByUserIdAsync(userId);
                return Ok(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting children for user {userId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Lấy từng trẻ của user
        [HttpGet("{childId}/user/{userId}/Get child by childId")]
        public async Task<IActionResult> GetChildById(int childId, int userId)
        {
            try
            {
                if (!ValidateUserAccess(userId))
                {
                    return Forbid("Bạn không có quyền xem thông tin này");
                }

                var child = await _childService.GetChildByIdAsync(childId, userId);
                return Ok(child);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting child {childId} for user {userId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        
        [HttpPost("user/{userId}/Create new child")]
        public async Task<IActionResult> CreateChild(int userId, [FromBody] CreateChildDTO childDTO)
        {
            try
            {
                if (!ValidateUserAccess(userId))
                {
                    return Forbid("Bạn không có quyền thực hiện hành động này");
                }

                var child = await _childService.CreateChildAsync(userId, childDTO);
                return CreatedAtAction(nameof(GetChildById), new { childId = child.ChildId, userId }, child);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating child for user {userId}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }//


        /// Tìm trẻ theo tên
        [HttpGet("GetChildById/{userId}/Searchname")]
        public async Task<ActionResult> GetChildByName(String search, int userId)
        {
            try
            {
                var child = await _childrenService.SearchNameChild(search, userId);
                return Ok(child);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// Cập nhật thông tin của trẻ
        [HttpPut("users/{userId}/children/{childId}")]
        public async Task<ActionResult> UpdateChild(int userId, int childId, [FromBody] UpdateChildrenDTO updateDTO)
        {
            try 
            {
                var updatechild = await _childrenService.UpdateChildAsync(userId,childId,updateDTO);
                return Ok(updatechild);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Xóa trẻ theo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChild(int childId)
        {
            await _childrenService.DeleteChildAsync(childId);
            return NoContent();
        }
    }
}
