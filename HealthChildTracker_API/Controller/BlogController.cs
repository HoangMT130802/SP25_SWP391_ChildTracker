
using System.Security.Claims;
using BusinessLogic.DTOs.Blog;
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
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


        [HttpGet]
        public async Task<ActionResult> GetAllBlogs()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogAsync();
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


        [HttpPost("like/{blogId}")]
        public async Task<IActionResult> LikeBlog(int blogId)
        {
            try
            {
                var result = await _blogService.LikeBlog(blogId);
                if (result)
                {
                    return Ok("Đã like bài viết!");
                }
                else
                {
                    return BadRequest("Không thể like bài viết.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("approve/{blogId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveBlog(int blogId)
        {
            var result = await _blogService.ApproveBlogAsync(blogId);
            if (result) return Ok($"Bài viết {blogId} đã được duyệt.");
            return BadRequest($"Lỗi duyệt bài viết {blogId}.");
        }

        [HttpPut("reject/{blogId}")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectBlog(int blogId)
        {
            var result = await _blogService.RejectBlogAsync(blogId);
            if (result) return Ok($"Bài viết {blogId} đã từ chối.");
            return BadRequest($"Lỗi từ chối bài viết {blogId}.");
        }



        [HttpPost]
        //[Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult> CreateBlogAsync(int userId, [FromBody] CreateBlogDTO createBlog)
        {
            if (createBlog == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            // Kiểm tra xem user có quyền tạo blog không
                //var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                //if (userRole != "Admin" && userRole != "Doctor")
                //{
                    //return Forbid(); // 403 - Không có quyền
                //}
                // Tạo blog
            var createdBlog = await _blogService.CreateBlogAsync(userId, createBlog);

            return Ok(createdBlog);
        }


        [HttpPut("{blogId}")]
        //[Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult> UpdateBlogAsync(int blogId, [FromBody] UpdateBlogDTO updateBlog)
        {
            if (updateBlog == null)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });
            }

            await _blogService.UpdateBlogAsync(blogId, updateBlog);
            return NoContent();
        }

        [HttpDelete("{blogId}")]
        //[Authorize(Roles = "Admin,Doctor")]
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
