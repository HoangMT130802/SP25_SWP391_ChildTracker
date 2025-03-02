using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Blog;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Interfaces
{
    public interface IblogService
    {
        Task<IEnumerable<BlogDTO>> GetAllBlogAsync();
        Task<PaginatedList<BlogDTO>> GetAllBlogPaginatedAsync(int pageIndex, int pageSize);
        Task<BlogDTO> GetblogByIdAsync(int BlogId);
        Task<BlogDTO> CreateBlogAsync(int userId, CreateBlogDTO createBlog);
        Task<BlogDTO> UpdateBlogAsync(int blogId, UpdateBlogDTO updateBlog);
        Task<bool> DeleteBlogAsync(int blogId);
    }
}
