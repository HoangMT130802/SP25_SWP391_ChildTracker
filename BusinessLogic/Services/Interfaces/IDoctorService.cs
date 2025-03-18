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
        Task<DoctorDTO> CreateDoctorAsync(CreateDoctorDTO doctorDTO);
        Task<DoctorDTO> UpdateDoctorAsync(int doctorId, UpdateDoctorDTO doctorDTO);
        Task DeleteDoctorAsync(int doctorId);
    }
}
