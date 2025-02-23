using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Doctor;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;

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
