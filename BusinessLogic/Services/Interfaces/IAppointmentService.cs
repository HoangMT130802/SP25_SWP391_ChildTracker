using BusinessLogic.DTOs.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDTO>> GetAllAppointmentsAsync();
        Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDoctorIdAsync(int doctorId);
        Task<IEnumerable<AppointmentDTO>> GetAppointmentsByUserIdAsync(int userId);
        Task<IEnumerable<AppointmentDTO>> GetAppointmentsByChildIdAsync(int childId);
        Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId);
        Task<AppointmentDTO> CreateAppointmentAsync(CreateAppointmentDTO appointmentDTO);
        Task<AppointmentDTO> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDTO appointmentDTO);
        Task<bool> DeleteAppointmentAsync(int appointmentId);
    }
}
