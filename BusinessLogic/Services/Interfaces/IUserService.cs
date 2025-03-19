using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Authentication;
using DataAccess.Models;

namespace BusinessLogic.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDetailsDTO> UserDetail(UserDetailsDTO request);
        Task<UserDetailsDTO> UserUpdate(int userId, UserUpdateDTO request);
        Task DeleteUserAsync(int UserId);
    }
}
