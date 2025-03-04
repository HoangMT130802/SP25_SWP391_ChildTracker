using System.Reflection.Metadata;
using BusinessLogic.DTOs.Blog;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories;

using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementations
{
    public class BlogService : IblogService
    {
        private readonly IGenericRepository<Blog> _blogrepository;
        private readonly ILogger<BlogService> _logger;

        public BlogService(IGenericRepository<Blog> blogrepository, ILogger<BlogService> logger)
        {
            _blogrepository = blogrepository;
            _logger = logger;
        }

        // Lấy allBlog chờ duyệt
        public async Task<IEnumerable<BlogDTO>> GetAllBlogPendingAsync()
        {
            try
            {
                var blogs = await _blogrepository.FindAsync(b => b.Status == "Pending");

                if (blogs == null || !blogs.Any())
                {
                    throw new Exception("No Blog in system.");
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
                throw new Exception("Unable to get list of blog", ex);
            }
        }


        // Lấy allBlog đã dc duyệt
        public async Task<IEnumerable<BlogDTO>> GetAllBlogApprovedAsync()
        {
            try
            {
                var blogs = await _blogrepository.FindAsync(b => b.Status == "Approved");

                if (blogs == null || !blogs.Any())
                {
                    throw new Exception("No Blog in system.");
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
                throw new Exception("Unable to get list of blog", ex);
            }
        }
        // lấy allblog có phân trang (đã dc duyệt)
        public async Task<PaginatedList<BlogDTO>> GetAllBlogPaginatedAsync(int pageIndex, int pageSize)
        {
            try
            {
                var query = _blogrepository.GetAllQueryable().Where(b => b.Status == "Approved");

                if (!query.Any())
                {
                    _logger.LogWarning("No Blog in system.");
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
                _logger.LogError(ex, "Error when getting list of paginated blog.");
                return new PaginatedList<BlogDTO>(new List<BlogDTO>(), 0, pageIndex, pageSize);
            }
        }

        //  Blog theo ID
        public async Task<BlogDTO> GetblogByIdAsync(int blogId)
        {
            try
            {
                var blog = await _blogrepository.GetAsync(b => b.BlogId == blogId && b.Status == "Approved");
                if (blog == null)
                {
                    throw new Exception($"No Blog {blogId} in system.");
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
                _logger.LogError(ex, $"Error while processing blog {blogId}");
                throw;
            }
        }


        // Blog theo từ khóa
        public async Task<List<BlogDTO>> SearchBlogByKeyword(string keyword)
        {
            try
            {
                keyword = keyword.ToLower();
                // Chỉ tìm blog có Status là "Approved" và chứa từ khóa trong Title hoặc Content
                var blogkeyword = await _blogrepository.FindAsync(b =>
                    b.Status == "Approved" &&
                    (b.Title.ToLower().Contains(keyword) || b.Content.ToLower().Contains(keyword)));

                if (!blogkeyword.Any())
                {
                    throw new Exception($"There are no posts containing the keyword '{keyword}'.");
                }
                return blogkeyword.Select(b => new BlogDTO
                {
                    BlogId = b.BlogId,
                    AuthorId = b.AuthorId,
                    Title = b.Title,
                    Content = b.Content,
                    ImageUrl = b.ImageUrl,
                    Views = b.Views,
                    Likes = b.Likes,
                    CreatedAt = b.CreatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during search: {ex.Message}");
            }
        }


        // Kiểm duyệt nội dung 
        public async Task<bool> ApproveBlogAsync(int blogId)
        {
            try
            {
                var blog = await _blogrepository.GetByIdAsync(blogId);
                if (blog.Status == "Pending")
                {
                    blog.Status = "Approved";
                    _blogrepository.Update(blog);
                    await _blogrepository.SaveAsync();
                    return true;
                }


                throw new Exception($"Blog {blogId} previously approved.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while browsing article {blogId}: {ex.Message}");

            }
        }
        public async Task<bool> RejectBlogAsync(int blogId)
        {
            try
            {
                var blog = await _blogrepository.GetByIdAsync(blogId);
                if (blog.Status == "Pending")
                {
                    blog.Status = "Rejected";
                    _blogrepository.Update(blog);
                    await _blogrepository.SaveAsync();
                    return true;
                }
                throw new Exception($"Blog {blogId} previously rejected.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when rejecting article {blogId}: {ex.Message}");
                return false;
            }
        }



        // tạo blog
        public async Task<BlogDTO> CreateBlogAsync(int userId, CreateBlogDTO createBlog)
        {
            try
            {

                var blog = new Blog
                {

                    AuthorId = userId,
                    Title = createBlog.Title,
                    Content = createBlog.Content,
                    ImageUrl = createBlog.ImageUrl,
                    Views = 0, // Mặc định ban đầu 
                    Likes = 0, // Mặc định ban đầu
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                await _blogrepository.InsertAsync(blog);

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
            catch (Exception ex)
            {

                throw new Exception($"Error When Creating Blog: {ex.Message}", ex);
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
                    throw new Exception($"No blog found {blogId}");
                }
                // Cập nhật blog
                blog.Title = updateBlog.Title;
                blog.Content = updateBlog.Content;
                blog.ImageUrl = updateBlog.ImageUrl;
                blog.Status = "Pending"; // Chuyển trạng thái về Pending để chờ duyệt

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
                throw new Exception("Error when update");
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
                    throw new Exception($"No blog found {blogId}");
                }

                _blogrepository.Delete(Blog);
                await _blogrepository.SaveAsync();
                return true;
            }
            catch
            {
                throw new Exception($"Error detele blog {blogId}");
            }

        }
    }
}
