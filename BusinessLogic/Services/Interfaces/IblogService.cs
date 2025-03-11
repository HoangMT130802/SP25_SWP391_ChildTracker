using BusinessLogic.DTOs.Blog;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Interfaces
{
    public interface IblogService
    {
        Task<IEnumerable<BlogDTO>> GetAllBlogPendingAsync();
        Task<IEnumerable<BlogDTO>> GetAllBlogApprovedAsync();
        Task<PaginatedList<BlogDTO>> GetAllBlogPaginatedAsync(int pageIndex, int pageSize);
        Task<BlogDTO> GetblogByIdAsync(int BlogId);
        Task<List<BlogDTO>> SearchBlogByKeyword(string keyword);
        Task<bool> ApproveBlogAsync(int blogId);
        Task<bool> RejectBlogAsync(int blogId);
        Task<BlogDTO> CreateBlogAsync(int userId, CreateBlogDTO createBlog);
        Task<BlogDTO> UpdateBlogAsync(int blogId, UpdateBlogDTO updateBlog, int currentUserId, string currentUserRole);
        Task<bool> DeleteBlogAsync(int blogId);
    }
}
