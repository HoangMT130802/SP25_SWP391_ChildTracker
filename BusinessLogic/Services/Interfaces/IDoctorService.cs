
using BusinessLogic.DTOs.Doctor;


namespace BusinessLogic.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDTO>> GetAllDoctorsAsync();
        Task<DoctorDTO> GetDoctorByIdAsync(int doctorId);
        Task<DoctorDTO> CreateDoctorAsync(CreateDoctorDTO doctorDTO);
        Task<DoctorDTO> UpdateDoctorAsync(int doctorId, UpdateDoctorDTO doctorDTO);
        Task<bool> ToggleDoctorVerification(int doctorId);
    }
}
