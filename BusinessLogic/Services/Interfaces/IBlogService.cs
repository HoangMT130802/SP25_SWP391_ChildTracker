﻿using BusinessLogic.DTOs.Blog;

namespace BusinessLogic.Services.Interfaces
{
    public interface IBlogService
    {
        Task<BlogDTO> CreateBlogAsync(int authorId, CreateBlogDTO blogDto);
        Task<BlogDTO> UpdateBlogAsync(int blogId, int userId, string userRole, UpdateBlogDTO blogDto);
        Task DeleteBlogAsync(int blogId, int userId, string userRole);
        Task<BlogDTO> GetBlogByIdAsync(int blogId);
        Task<IEnumerable<BlogDTO>> GetAllBlogsAsync();
        Task<IEnumerable<BlogDTO>> GetBlogsByAuthorIdAsync(int authorId);
    }
}
