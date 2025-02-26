using AutoMapper;
using BusinessLogic.DTOs.Doctor;
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
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DoctorService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<DoctorDTO>> GetAllDoctorsAsync()
        {
            try
            {
                var userRepository = _unitOfWork.GetRepository<User>();
                // Chỉ lấy các user có role là Doctor và include DoctorProfiles
                var doctors = await userRepository.FindAsync(
                    u => u.Role == "Doctor",
                    includeProperties: "DoctorProfiles");
                return _mapper.Map<IEnumerable<DoctorDTO>>(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all doctors");
                throw;
            }
        }

        public async Task<DoctorDTO> GetDoctorByIdAsync(int doctorId)
        {
            try
            {
                var userRepository = _unitOfWork.GetRepository<User>();
                // Chỉ lấy user có role là Doctor và include DoctorProfiles
                var doctor = await userRepository.GetAsync(
                    u => u.UserId == doctorId && u.Role == "Doctor",
                    includeProperties: "DoctorProfiles");

                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Doctor with ID {doctorId} not found");
                }

                return _mapper.Map<DoctorDTO>(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting doctor {doctorId}");
                throw;
            }
        }

        public async Task<DoctorDTO> CreateDoctorAsync(CreateDoctorDTO doctorDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Kiểm tra username và email đã tồn tại chưa
                var userRepository = _unitOfWork.GetRepository<User>();
                var existingUser = await userRepository.GetAsync(u =>
                    u.Username == doctorDTO.Username || u.Email == doctorDTO.Email);

                if (existingUser != null)
                {
                    throw new InvalidOperationException("Username or email already exists");
                }

                // Tạo user mới với role Doctor
                var user = new User
                {
                    Username = doctorDTO.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(doctorDTO.Password),
                    Email = doctorDTO.Email,
                    FullName = doctorDTO.FullName,
                    Phone = doctorDTO.Phone,
                    Address = doctorDTO.Address,
                    Role = "Doctor", // Đảm bảo role luôn là Doctor
                    Status = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await userRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Tạo doctor profile
                var profileRepository = _unitOfWork.GetRepository<DoctorProfile>();
                var profile = new DoctorProfile
                {
                    UserId = user.UserId,
                    Specialization = doctorDTO.Specialization,
                    Qualification = doctorDTO.Qualification,
                    Experience = doctorDTO.Experience,
                    LicenseNumber = doctorDTO.LicenseNumber,
                    Biography = doctorDTO.Biography,
                    AverageRating = 0,
                    TotalRatings = 0,
                    IsVerified = false
                };

                await profileRepository.AddAsync(profile);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // Load lại user với profile để trả về
                var createdDoctor = await userRepository.GetAsync(
                    u => u.UserId == user.UserId,
                    includeProperties: "DoctorProfiles");
                return _mapper.Map<DoctorDTO>(createdDoctor);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating doctor");
                throw;
            }
        }
        public async Task<DoctorDTO> UpdateDoctorAsync(int doctorId, UpdateDoctorDTO doctorDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Lấy thông tin doctor
                var userRepository = _unitOfWork.GetRepository<User>();
                var doctor = await userRepository.GetAsync(u => u.UserId == doctorId && u.Role == "Doctor");

                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Doctor with ID {doctorId} not found");
                }

                // Cập nhật thông tin user
                doctor.Email = doctorDTO.Email;
                doctor.FullName = doctorDTO.FullName;
                doctor.Phone = doctorDTO.Phone;
                doctor.Address = doctorDTO.Address;
                doctor.Status = doctorDTO.Status;
                doctor.UpdatedAt = DateTime.UtcNow;

                userRepository.Update(doctor);
                await _unitOfWork.SaveChangesAsync();

                // Cập nhật thông tin profile
                var profileRepository = _unitOfWork.GetRepository<DoctorProfile>();
                var profile = await profileRepository.GetAsync(p => p.UserId == doctorId);

                if (profile != null)
                {
                    profile.Specialization = doctorDTO.Specialization;
                    profile.Qualification = doctorDTO.Qualification;
                    profile.Experience = doctorDTO.Experience;
                    profile.LicenseNumber = doctorDTO.LicenseNumber;
                    profile.Biography = doctorDTO.Biography;
                    profile.IsVerified = doctorDTO.IsVerified;

                    profileRepository.Update(profile);
                    await _unitOfWork.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Load lại user với profile để trả về
                var updatedDoctor = await userRepository.GetAsync(u => u.UserId == doctorId, "DoctorProfiles");
                return _mapper.Map<DoctorDTO>(updatedDoctor);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating doctor {doctorId}");
                throw;
            }
        }

        public async Task<bool> DeleteDoctorAsync(int doctorId)
        {
            try
            {
                var userRepository = _unitOfWork.GetRepository<User>();
                var doctor = await userRepository.GetAsync(u => u.UserId == doctorId && u.Role == "Doctor");

                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Doctor with ID {doctorId} not found");
                }

                // Soft delete
                doctor.Status = false;
                doctor.UpdatedAt = DateTime.UtcNow;

                userRepository.Update(doctor);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting doctor {doctorId}");
                throw;
            }
        }
    }
}
