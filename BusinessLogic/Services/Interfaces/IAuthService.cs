using BusinessLogic.DTOs.Authentication;
using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface  IAuthService
    {
        Task<UserResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<UserResponseDTO> RegisterAsync(RegisterRequestDTO request);
        Task ValidateRegistrationRequest(RegisterRequestDTO request);
    }
}
