using BusinessLogic.DTOs.Blog;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthChildTracker_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogDTO blogDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return Unauthorized("Không tìm thấy thông tin người dùng");

                var blog = await _blogService.CreateBlogAsync(userId.Value, blogDto);
                return CreatedAtAction(nameof(GetBlog), new { blogId = blog.BlogId }, blog);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo blog: {ex.Message}");
            }
        }

        [HttpPut("{blogId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateBlog(int blogId, [FromBody] UpdateBlogDTO blogDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                if (!userId.HasValue || string.IsNullOrEmpty(userRole))
                    return Unauthorized("Không tìm thấy thông tin người dùng");

                var blog = await _blogService.UpdateBlogAsync(blogId, userId.Value, userRole, blogDto);
                return Ok(blog);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật blog: {ex.Message}");
            }
        }

        [HttpDelete("{blogId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteBlog(int blogId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                if (!userId.HasValue || string.IsNullOrEmpty(userRole))
                    return Unauthorized("Không tìm thấy thông tin người dùng");

                await _blogService.DeleteBlogAsync(blogId, userId.Value, userRole);
                return Ok("Bài biết đã được xoá");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa blog: {ex.Message}");
            }
        }

        [HttpGet("{blogId}")]
        public async Task<IActionResult> GetBlog(int blogId)
        {
            try
            {
                var blog = await _blogService.GetBlogByIdAsync(blogId);
                return Ok(blog);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy thông tin blog: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogs()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogsAsync();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách blog: {ex.Message}");
            }
        }

        [HttpGet("author/{authorId}")]
        public async Task<IActionResult> GetBlogsByAuthor(int authorId)
        {
            try
            {
                var blogs = await _blogService.GetBlogsByAuthorIdAsync(authorId);
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách blog của tác giả: {ex.Message}");
            }
        }
    }
}
