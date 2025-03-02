using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using BusinessLogic.DTOs.Blog;
using BusinessLogic.DTOs.Children;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementations
{
    public class BlogService : IblogService
    {
        private readonly IGenericRepository<Blog> _blogrepository;
        private readonly ILogger<BlogService> _logger;
        public BlogService(IGenericRepository<Blog> blogrepository)
        {
            _blogrepository = blogrepository;
        }


        // Lấy allBlog
        public async Task<IEnumerable<BlogDTO>> GetAllBlogAsync()
        {
            try
            {
                var blogs = await _blogrepository.GetAllAsync();

                if (blogs == null || !blogs.Any())
                {
                    throw new Exception("Không có bài viết nào trong hệ thống.");
                }
                return blogs.Select(blog => new BlogDTO
                {
                    BlogId = blog.BlogId,
                    AuthorId = blog.AuthorId,
                    Title = blog.Title,
                    Content = blog.Content,
                    ImageUrl = blog.ImageUrl,
                    Views = blog.Views,
                    Likes = blog.Likes,
                    CreatedAt = blog.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể lấy danh sách bài viết", ex);
            }
        }
        // lấy allblog có phân trang
        public async Task<PaginatedList<BlogDTO>> GetAllBlogPaginatedAsync(int pageIndex, int pageSize)
        {
            try
            {
                var query = _blogrepository.GetAllQueryable();

                if (!query.Any())
                {
                    _logger.LogWarning("Không có bài viết nào trong hệ thống.");
                    return new PaginatedList<BlogDTO>(new List<BlogDTO>(), 0, pageIndex, pageSize);
                }
                // Chuyển Blog -> BlogDTO trước khi phân trang
                var blogDTOQuery = query.Select(blog => new BlogDTO
                {
                    BlogId = blog.BlogId,
                    AuthorId = blog.AuthorId,
                    Title = blog.Title,
                    Content = blog.Content,
                    ImageUrl = blog.ImageUrl,
                    Views = blog.Views,
                    Likes = blog.Likes,
                    CreatedAt = blog.CreatedAt
                });
                // Phân trang 
                var paginatedBlogs = await Task.Run(() => PaginatedList<BlogDTO>.Create(blogDTOQuery, pageIndex, pageSize));
                return paginatedBlogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bài viết có phân trang.");
                return new PaginatedList<BlogDTO>(new List<BlogDTO>(), 0, pageIndex, pageSize);
            }
        }

        //  Blog theo ID
        public async Task<BlogDTO> GetblogByIdAsync(int blogId)
        {
            try
            {
                var blog = await _blogrepository.GetByIdAsync(blogId);
                if (blog == null)
                {
                    throw new Exception($"Không có bài viết {blogId} trong hệ thống.");
                }
                return new BlogDTO
                {
                    BlogId = blog.BlogId,
                    AuthorId = blog.AuthorId,
                    Title = blog.Title,
                    Content = blog.Content,
                    ImageUrl = blog.ImageUrl,
                    Views = blog.Views,
                    Likes = blog.Likes,
                    CreatedAt = blog.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xử bài viết {blogId}");
                throw;
            }
        }

        // tạo blog
        public async Task<BlogDTO> CreateBlogAsync(int userId, CreateBlogDTO createBlog)
        {
            try
            {
                //Lấy giá trị BlogId lớn nhất hiện tại
                int maxBlogId = await _blogrepository.GetAllQueryable().MaxAsync(b => (int?)b.BlogId) ?? 0;
                var blog = new Blog
                {
                    BlogId = maxBlogId + 1,  //Tự động tăng
                    AuthorId = userId,
                    Title = createBlog.Title,
                    Content = createBlog.Content,
                    ImageUrl = createBlog.ImageUrl,
                    Views = 0, // Mặc định ban đầu 
                    Likes = 0, // Mặc định ban đầu
                    Status = "true",
                    CreatedAt = DateTime.UtcNow
                };

                await _blogrepository.AddAsync(blog);
                await _blogrepository.SaveAsync();

                return new BlogDTO
                {
                    BlogId = blog.BlogId,
                    AuthorId = blog.AuthorId,
                    Title = blog.Title,
                    Content = blog.Content,
                    ImageUrl = blog.ImageUrl,
                    Views = blog.Views,
                    Likes = blog.Likes,
                    CreatedAt = blog.CreatedAt
                };
            }
            catch(Exception ex)
            {
                
                throw new Exception("Lỗi khi tạo bài viết");
            }
        }

        // cập nhật blog
        public async Task<BlogDTO> UpdateBlogAsync(int blogId, UpdateBlogDTO updateBlog)
        {
            try
            {
                var blog = await _blogrepository.GetByIdAsync(blogId);
                if (blog == null)
                {
                    throw new Exception($"Không tìm thấy bài viết {blogId}");
                }
                // Cập nhật blog
                blog.Title = updateBlog.Title;
                blog.Content = updateBlog.Content;
                blog.ImageUrl = updateBlog.ImageUrl;

                _blogrepository.Update(blog);
                await _blogrepository.SaveAsync();

                return new BlogDTO
                {
                    BlogId = blog.BlogId,
                    AuthorId = blog.AuthorId,
                    Title = blog.Title,
                    Content = blog.Content,
                    ImageUrl = blog.ImageUrl,
                    Views = blog.Views,
                    Likes = blog.Likes,
                    CreatedAt = blog.CreatedAt
                };
            }
            catch
            {
                throw new Exception("Lỗi khi update bài viết");
            }
        }

        // xóa blog 
        public async Task<bool> DeleteBlogAsync(int blogId)
        {
            try
            {
                var Blog = await _blogrepository.GetAsync(b => b.BlogId == blogId);

                if (Blog == null)
                {
                    throw new Exception($"Không tìm thấy bài viết {blogId}");
                }

                _blogrepository.Delete(Blog);
                await _blogrepository.SaveAsync();
                return true;
            }
            catch
            {
                throw new Exception($"Lỗi khi xóa bài viết {blogId}");
            }

        }
    }
}
