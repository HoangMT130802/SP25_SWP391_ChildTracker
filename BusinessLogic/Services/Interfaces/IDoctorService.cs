using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Doctor;
using DataAccess.Entities;

namespace BusinessLogic.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDTO>> GetAllDoctorsAsync();
        Task<DoctorDTO> GetDoctorByIdAsync(int doctorId);
        Task<List<DoctorDTO>> SearchSpecialization(String search);
        Task CreateDoctorAsync(CreateDoctorDTO doctorDto);
        Task UpdateDoctorAsync(int doctorId, UpdateDoctorDTO doctorDto);
        Task DeleteDoctorAsync(int doctorId);
    }
}
