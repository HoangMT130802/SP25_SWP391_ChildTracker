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
        Task<IEnumerable<AppointmentDTO>> GetUserAppointmentsAsync(int userId);
        Task<IEnumerable<AppointmentDTO>> GetDoctorAppointmentsAsync(int doctorId);
        Task<IEnumerable<AppointmentDTO>> GetChildAppointmentsAsync(int childId);
        Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId);
        Task<AppointmentDTO> CreateAppointmentAsync(CreateAppointmentDTO appointmentDTO);
        Task<AppointmentDTO> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDTO appointmentDTO);
        Task<bool> CancelAppointmentAsync(int appointmentId, int userId);
        Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<AppointmentDTO>> GetDoctorAppointmentsByDateAsync(int doctorId, DateOnly date);
    }
}
