using AutoMapper;
using BusinessLogic.DTOs.User;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<UserDTO>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            return _mapper.Map<List<UserDTO>>(users);
        }

        public async Task<UserDTO> GetCurrentUserDetailAsync(int currentUserId)
        {
            var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(currentUserId);
            if (user == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy thông tin người dùng hiện tại");
            }
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> UpdateUserProfileAsync(int userId, UpdateUserProfileDTO updateUserDTO)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");
            }

            _mapper.Map(updateUserDTO, user);
            user.UpdatedAt = DateTime.UtcNow;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool status)
        {
            var userRepo = _unitOfWork.GetRepository<User>();
            var user = await userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");
            }

            user.Status = status;
            user.UpdatedAt = DateTime.UtcNow;

            userRepo.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
