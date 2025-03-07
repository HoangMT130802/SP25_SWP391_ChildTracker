using BusinessLogic.DTOs.Authentication;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserResponseDTO> LoginAsync(LoginRequestDTO request);
        Task<UserResponseDTO> RegisterAsync(RegisterRequestDTO request);
        Task<bool> LogoutAsync(string sessionId);
        Task ValidateRegistrationRequest(RegisterRequestDTO request);
    }
}
