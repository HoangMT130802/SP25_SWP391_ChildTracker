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

namespace BusinessLogic.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;

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

                _logger.LogInformation($"User {user.Username} logged in successfully");
                return _mapper.Map<UserResponseDTO>(user);
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

               
                await userRepository.AddAsync(newUser);

                
                var result = await _unitOfWork.SaveChangesAsync();

                if (result <= 0)
                {
                    throw new Exception("Không thể lưu user mới vào database");
                }

                _logger.LogInformation($"User {newUser.Username} registered successfully with ID: {newUser.UserId}");

                return _mapper.Map<UserResponseDTO>(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration failed: {ex.Message}");
                throw;
            }
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

        private string HashPassword(string password)
        {            
            return password; 
        }
    }
}
