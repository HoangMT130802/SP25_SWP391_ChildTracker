using BusinessLogic.DTOs.Children;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildrenController : ControllerBase
    {
        private readonly IChildrenService _childrenService;

        public ChildrenController(IChildrenService childrenService)
        {
            _childrenService = childrenService;
        }

        /// Lấy danh sách tất cả trẻ em của user 
        [HttpGet("users/{userId}/children")]
        public async Task<IActionResult> GetAllChildrenByUser(int userId)
        {
            var children = await _childrenService.GetChildrenByUserIdAsync(userId);

            if (children == null || !children.Any())
            {
                return NotFound("Không tìm thấy trẻ ");
            }

            return Ok(children);
        }

        // Lấy ds trẻ có phân trang
        [HttpGet("{userId}/children")]
        public async Task<IActionResult> GetChildrenPagination(int userId, int page = 1, int pageSize = 5)
        {
            var paginatedChildren = await _childrenService.GetChildrenPaginationAsync(userId, page, pageSize);
            return Ok(new
            {
                CurrentPage = paginatedChildren.PageIndex,
                TotalPages = paginatedChildren.TotalPage,
                PageSize = pageSize,
                Children = paginatedChildren
            });
        }

        /// Tìm trẻ theo id
        [HttpGet("GetChildById/{userId}/{childId}")]
        public async Task<ActionResult> GetChildById(int userId, int childId)
        {
            try
            {
                var child = await _childrenService.GetChildByIdAsync(userId, childId);
                return Ok(child);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


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

        /// Tạo mới trẻ
        [HttpPost("users/{userId}/children")]
        public async Task<ActionResult> CreateChild([FromBody] CreateChildrenDTO create, int userId)
        {
            try
            {
                var newChild = await _childrenService.CreateChildAsync(create, userId);
                return Ok(newChild);
            }
            catch(Exception ex)
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
