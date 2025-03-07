using AutoMapper;
using BusinessLogic.DTOs.Authentication;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace BusinessLogic.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private static readonly ConcurrentDictionary<string, UserSession> _sessions = new();

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserResponseDTO> LoginAsync(LoginRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UsernameOrEmail) || string.IsNullOrEmpty(request.Password))
                {
                    throw new ArgumentException("Username/email và mật khẩu không được để trống");
                }

                var userRepository = _unitOfWork.GetRepository<User>();

                var user = await userRepository.GetAsync(u =>
                    (u.Username.ToLower() == request.UsernameOrEmail.ToLower() ||
                     u.Email.ToLower() == request.UsernameOrEmail.ToLower())
                    && u.Password == request.Password  // Nên hash password
                    && u.Status == true);

                if (user == null)
                {
                    throw new UnauthorizedAccessException("Thông tin đăng nhập không chính xác");
                }

                // Tạo session mới
                var sessionId = Guid.NewGuid().ToString();
                var session = new UserSession
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role,
                    CreatedAt = DateTime.UtcNow
                };

                _sessions.TryAdd(sessionId, session);

                var response = _mapper.Map<UserResponseDTO>(user);
                response.SessionId = sessionId;

                _logger.LogInformation($"User {user.Username} logged in successfully");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login failed: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDTO> RegisterAsync(RegisterRequestDTO request)
        {
            try
            {
                await ValidateRegistrationRequest(request);

                var userRepository = _unitOfWork.GetRepository<User>();
                var newUser = _mapper.Map<User>(request);
                newUser.CreatedAt = DateTime.UtcNow;
                newUser.UpdatedAt = DateTime.UtcNow;
                newUser.Status = true; // Mặc định active

                await userRepository.AddAsync(newUser);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"User {newUser.Username} registered successfully");
                return _mapper.Map<UserResponseDTO>(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> LogoutAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return false;
            }

            return _sessions.TryRemove(sessionId, out _);
        }

        public async Task ValidateRegistrationRequest(RegisterRequestDTO request)
        {
            var userRepository = _unitOfWork.GetRepository<User>();

            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Username và password không được để trống");
            }

            if (!IsValidEmail(request.Email))
            {
                throw new ArgumentException("Email không hợp lệ");
            }

            if (await userRepository.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                throw new InvalidOperationException("Username đã tồn tại");
            }

            if (await userRepository.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                throw new InvalidOperationException("Email đã tồn tại");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    public class UserSession
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
