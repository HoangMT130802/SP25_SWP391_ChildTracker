using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Doctor;
using DataAccess.Models;

namespace BusinessLogic.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorProfile>> GetAllDoctorsAsync();
        Task<DoctorProfile> GetDoctorByIdAsync(int doctorId);
        Task<List<DoctorDTO>> SearchNameDoctor(String search);
        Task<List<DoctorDTO>> SearchSpecialization(String search);
        Task CreateDoctorAsync(CreateDoctorDTO doctorDto);
        Task UpdateDoctorAsync(int doctorId, UpdateDoctorDTO doctorDto);
        Task DeleteDoctorAsync(int doctorId);
    }
}
