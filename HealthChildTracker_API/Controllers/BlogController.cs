using System.Security.Claims;
using BusinessLogic.DTOs.Blog;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace API.Controllers
{
    [Route("api/blog")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IblogService _blogService;

        public BlogController(IblogService blogService)
        {
            _blogService = blogService;
        }

        [HttpGet("AllBlogPending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetAllBlogPendingAsync()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogPendingAsync();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("AllBlogApproved")]
        public async Task<ActionResult> GetAllBlogApprovedAsync()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogApprovedAsync();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("{blogId}")]
        public async Task<ActionResult> GetBlogById(int blogId)
        {
            try
            {
                var blog = await _blogService.GetblogByIdAsync(blogId);
                return Ok(blog);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpGet("paged")]
        public async Task<ActionResult> GetAllBlogsPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 3)
        {
            try
            {
                var paginatedBlogs = await _blogService.GetAllBlogPaginatedAsync(pageIndex, pageSize);
                // Tạo object chứa kết quả phân trang
                var result = new
                {
                    PageIndex = pageIndex,                     // Trang hiện tại
                    PageSize = pageSize,                       // Số lượng trên mỗi trang
                    Blogs = paginatedBlogs               // Danh sách các bài viết
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("searchblogkeyword")]
        public async Task<IActionResult> SearchBlogskeyword([FromQuery] string keyword)
        {
            try
            {
                var result = await _blogService.SearchBlogByKeyword(keyword);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPut("approve/{blogId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveBlog(int blogId)
        {
            var result = await _blogService.ApproveBlogAsync(blogId);
            if (result) return Ok($"Bài viết {blogId} đã được duyệt.");
            return BadRequest($"Lỗi duyệt bài viết {blogId}.");
        }

        [HttpPut("reject/{blogId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectBlog(int blogId)
        {
            var result = await _blogService.RejectBlogAsync(blogId);
            if (result) return Ok($"Bài viết {blogId} đã từ chối.");
            return BadRequest($"Lỗi từ chối bài viết {blogId}.");
        }


        [HttpPost("CreateBlog")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult> CreateBlogAsync([FromBody] CreateBlogDTO createBlog)
        {
            if (createBlog == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            // Lấy userId từ token
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng" });
            }

            int userId = int.Parse(userIdClaim.Value);
            var createdBlog = await _blogService.CreateBlogAsync(userId, createBlog);

            return Ok(createdBlog);
        }


        [HttpPut("UpdateBlog/{blogId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult> UpdateBlogAsync(int blogId, [FromBody] UpdateBlogDTO updateBlog)
        {
            if (updateBlog == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            // Lấy UserId và Role từ JWT Token
            int currentUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            string currentUserRole = User.FindFirst("role")?.Value ?? "";

            try
            {
                // Chuyển xuống Service xử lý
                await _blogService.UpdateBlogAsync(blogId, updateBlog, currentUserId, currentUserRole);
                return Ok(new { message = "Cập nhật thành công" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid(); // HTTP 403: Không có quyền
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpDelete("{blogId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult> DeleteDoctor(int blogId)
        {
            try
            {
                await _blogService.DeleteBlogAsync(blogId);
                return Ok(new { message = "Delete successful" });
            }
            catch
            {
                return BadRequest(new { message = "Delete failed" });
            }
        }
    }
}
