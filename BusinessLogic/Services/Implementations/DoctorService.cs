
using AutoMapper;
using BusinessLogic.DTOs.Children;
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories;
using DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                var doctors = await userRepository.FindAsync(
                    u => u.Role == "Doctor",
                    includeProperties: "DoctorProfiles");
                return _mapper.Map<IEnumerable<DoctorDTO>>(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bác sĩ");
                throw;
            }
        }

        public async Task<DoctorDTO> GetDoctorByIdAsync(int doctorId)
        {
            try
            {
                var userRepository = _unitOfWork.GetRepository<User>();
                var doctor = await userRepository.GetAsync(
                    u => u.UserId == doctorId && u.Role == "Doctor",
                    includeProperties: "DoctorProfiles");

                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy bác sĩ với ID {doctorId}");
                }

                return _mapper.Map<DoctorDTO>(doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin bác sĩ {doctorId}");
                throw;
            }
        }

        // tìm kiếm theo chuyên môn 
        public async Task<List<DoctorDTO>> SearchSpecialization(String search)
        {
            var result = await _doctorRepository.GetAllQueryable()
                .Include(d => d.User)
                .Where(d => d.Specialization.ToLower().Contains(search.ToLower()))
                .ToListAsync();
            if (!result.Any())
            {
                throw new Exception("Không tìm thấy bác sĩ");
            }

            return result.Select(d => new DoctorDTO
            {
                FullName = d.User.FullName, // tên bác sĩ từ User
                DoctorProfile = new DoctorProfileDTO
                {
                    DoctorProfileId = d.DoctorProfileId,
                    Specialization = d.Specialization,
                    Qualification = d.Qualification,
                    Experience = d.Experience,
                    LicenseNumber = d.LicenseNumber,
                    Biography = d.Biography,
                    AverageRating = d.AverageRating,
                    TotalRatings = d.TotalRatings
                }
            }).ToList();
        }

        public async Task CreateDoctorAsync(CreateDoctorDTO doctorDto)
        {
            // Kiểm tra xem User có tồn tại không
            var user = await _userRepository.GetByIdAsync(doctorDto.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Cập nhật role của User thành Doctor
            user.Role = "Doctor";
            _userRepository.Update(user);

            // Tạo hồ sơ bác sĩ
            var doctorProfile = new DoctorProfile
            {
                UserId = doctorDto.UserId,
                Specialization = doctorDto.Specialization,
                Qualification = doctorDto.Qualification,
                Experience = doctorDto.Experience,
                LicenseNumber = doctorDto.LicenseNumber,
                Biography = doctorDto.Biography,
                IsVerified = false
            };

            await _doctorRepository.AddAsync(doctorProfile);
            await _doctorRepository.SaveAsync();
        }

        public async Task UpdateDoctorAsync(int doctorId, UpdateDoctorDTO doctorDto)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found");
            }

            doctor.Specialization = doctorDto.Specialization;
            doctor.Qualification = doctorDto.Qualification;
            doctor.Experience = doctorDto.Experience;
            doctor.LicenseNumber = doctorDto.LicenseNumber;
            doctor.Biography = doctorDto.Biography;
            doctor.IsVerified = doctorDto.IsVerified;

            _doctorRepository.Update(doctor);
            await _doctorRepository.SaveAsync();
        }

        public async Task DeleteDoctorAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
            {
                throw new KeyNotFoundException("Doctor not found");
            }

            _doctorRepository.Delete(doctor);
            await _doctorRepository.SaveAsync();
        }
    }
}
