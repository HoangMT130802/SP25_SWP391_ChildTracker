
using BusinessLogic.DTOs.Children;
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IGenericRepository<DoctorProfile> _doctorRepository;
        private readonly IGenericRepository<User> _userRepository;

        public DoctorService(
            IGenericRepository<DoctorProfile> doctorRepository,
            IGenericRepository<User> userRepository)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<DoctorProfile>> GetAllDoctorsAsync()
        {
            return await _doctorRepository.GetAllAsync(); 
        }

        public async Task<DoctorProfile> GetDoctorByIdAsync(int doctorId)
        {
            return await _doctorRepository.GetByIdAsync(doctorId);
        }

        // tìm kiếm theo tên bác sĩ
        public async Task<List<DoctorDTO>> SearchNameDoctor(String search)
        {
            var result = await _userRepository.GetAllQueryable()
                .Include(u => u.DoctorProfiles)
                .Where(u => u.FullName.ToLower().Contains(search.ToLower()) && u.Role == "Doctor")
                .ToListAsync();
            if (!result.Any()) 
            {
                throw new Exception("Không tìm thấy bác sĩ");
            }

            return result.Select(u => new DoctorDTO
            {
                FullName = u.FullName,
                DoctorProfile = new DoctorProfileDTO
                {
                    DoctorProfileId = u.DoctorProfiles.First().DoctorProfileId,
                    Specialization = u.DoctorProfiles.First().Specialization,
                    Qualification = u.DoctorProfiles.First().Qualification,
                    Experience = u.DoctorProfiles.First().Experience,
                    LicenseNumber = u.DoctorProfiles.First().LicenseNumber,
                    Biography = u.DoctorProfiles.First().Biography,
                    AverageRating = u.DoctorProfiles.First().AverageRating,
                    TotalRatings = u.DoctorProfiles.First().TotalRatings
                }
            }).ToList();
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
