using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _genericRepository;

        public UserService(IGenericRepository<User> genericRepository)
        {
            _genericRepository = genericRepository;
        }

        // Kiểm Tra UserID
        private async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId < 0)
            {
                throw new ArgumentException("UserId không hợp lệ.");
            }

            var user = await _genericRepository.GetAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new Exception("Người dùng không tồn tại.");
            }

            return user;
        }

        public async Task<UserDetailsDTO> UserDetail(UserDetailsDTO request)
        {
            var user = await GetUserByIdAsync(request.UserId);

            return new UserDetailsDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<UserDetailsDTO> UserUpdate(int userId, UserUpdateDTO request)
        {
            var user = await GetUserByIdAsync(userId);

            // Cập nhật thông tin từ request
            if (!string.IsNullOrWhiteSpace(request.Username))
                user.Username = request.Username;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.Phone))
                user.Phone = request.Phone;

            if (!string.IsNullOrWhiteSpace(request.Address))
                user.Address = request.Address;

            // Cập nhật UpdatedAt
            user.UpdatedAt = DateTime.UtcNow;

            // Cập nhật vào database
            _genericRepository.Update(user);
            await _genericRepository.SaveAsync();

            // Trả về UserDetailsDTO
            return new UserDetailsDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                Status = user.Status,
                CreatedAt = user.CreatedAt, // Giữ nguyên
                UpdatedAt = user.UpdatedAt   // Đã cập nhật
            };
        }
        public async Task DeleteUserAsync(int UserId)
        {
            var User = await _genericRepository.GetByIdAsync(UserId);
            if (User == null)
            {
                throw new KeyNotFoundException("Doctor not found");
            }

            _genericRepository.Delete(User);
            await _genericRepository.SaveAsync();
        }
    }
}
