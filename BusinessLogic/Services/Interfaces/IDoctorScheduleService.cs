using BusinessLogic.DTOs.Doctor_Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<IEnumerable<DoctorScheduleDTO>> GetAllSchedulesAsync();
        Task<IEnumerable<DoctorScheduleDTO>> GetDoctorSchedulesAsync(int doctorId);
        Task<IEnumerable<DoctorScheduleDTO>> GetDoctorSchedulesByDateRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate);
        Task<DoctorScheduleDTO> GetScheduleByIdAsync(int scheduleId);
        Task<DoctorScheduleDTO> CreateScheduleAsync(CreateDoctorScheduleDTO scheduleDTO);
        Task<DoctorScheduleDTO> UpdateScheduleAsync(int scheduleId, UpdateDoctorScheduleDTO scheduleDTO);
        Task<bool> DeleteScheduleAsync(int scheduleId);
        Task<IEnumerable<TimeSlotDTO>> GetAvailableSlotsAsync(int scheduleId);
        Task<bool> IsSlotAvailableAsync(int scheduleId, TimeOnly slotTime);
        Task<List<TimeSlotDTO>> CalculateAvailableSlotsAsync(int scheduleId);
    }
}
