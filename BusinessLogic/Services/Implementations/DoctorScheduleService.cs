using AutoMapper;
using BusinessLogic.DTOs.Doctor_Schedule;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorScheduleService> _logger;
        private const int SLOT_DURATION = 45; // Thời lượng slot cố định 45 phút
        private readonly TimeOnly WORK_START_TIME = new TimeOnly(8, 0); // Giờ bắt đầu làm việc
        private readonly TimeOnly WORK_END_TIME = new TimeOnly(17, 0); // Giờ kết thúc làm việc

        public DoctorScheduleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DoctorScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DoctorScheduleDTO>> GetAllSchedulesAsync()
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedules = await scheduleRepository.GetAllAsync(includeProperties: "Doctor");
                
                var scheduleDTOs = _mapper.Map<IEnumerable<DoctorScheduleDTO>>(schedules);
                foreach (var scheduleDTO in scheduleDTOs)
                {
                    scheduleDTO.AvailableSlots = await CalculateAvailableSlotsAsync(scheduleDTO.ScheduleId);
                }

                return scheduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả lịch làm việc của bác sĩ");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorScheduleDTO>> GetDoctorSchedulesAsync(int doctorId)
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedules = await scheduleRepository.FindAsync(
                    s => s.DoctorId == doctorId,
                    includeProperties: "Doctor"
                );

                var scheduleDTOs = _mapper.Map<IEnumerable<DoctorScheduleDTO>>(schedules);

                // Tính toán các slot có sẵn cho mỗi lịch
                foreach (var scheduleDTO in scheduleDTOs)
                {
                    scheduleDTO.AvailableSlots = await CalculateAvailableSlotsAsync(scheduleDTO.ScheduleId);
                }

                return scheduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedules for doctor {doctorId}");
                throw;
            }
        }

        public async Task<IEnumerable<DoctorScheduleDTO>> GetDoctorSchedulesByDateRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedules = await scheduleRepository.FindAsync(
                    s => s.DoctorId == doctorId && s.WorkDate >= startDate && s.WorkDate <= endDate,
                    includeProperties: "Doctor"
                );

                var scheduleDTOs = _mapper.Map<IEnumerable<DoctorScheduleDTO>>(schedules);

                // Tính toán các slot có sẵn cho mỗi lịch
                foreach (var scheduleDTO in scheduleDTOs)
                {
                    scheduleDTO.AvailableSlots = await CalculateAvailableSlotsAsync(scheduleDTO.ScheduleId);
                }

                return scheduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedules for doctor {doctorId} between {startDate} and {endDate}");
                throw;
            }
        }

        public async Task<DoctorScheduleDTO> GetScheduleByIdAsync(int scheduleId)
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedule = await scheduleRepository.GetAsync(
                    s => s.ScheduleId == scheduleId,
                    includeProperties: "Doctor"
                );

                if (schedule == null)
                {
                    throw new KeyNotFoundException($"Schedule with ID {scheduleId} not found");
                }

                var scheduleDTO = _mapper.Map<DoctorScheduleDTO>(schedule);
                scheduleDTO.AvailableSlots = await CalculateAvailableSlotsAsync(scheduleId);

                return scheduleDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedule {scheduleId}");
                throw;
            }
        }

        public async Task<DoctorScheduleDTO> CreateScheduleAsync(CreateDoctorScheduleDTO scheduleDTO)
        {
            try
            {
                // Kiểm tra ngày tạo lịch phải sau ngày hiện tại 3 ngày
                var minAllowedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
                if (scheduleDTO.WorkDate < minAllowedDate)
                {
                    throw new InvalidOperationException("Chỉ được tạo lịch trước 3 ngày");
                }

                // Kiểm tra bác sĩ có tồn tại không
                var userRepository = _unitOfWork.GetRepository<User>();
                var doctor = await userRepository.GetAsync(u => u.UserId == scheduleDTO.DoctorId && u.Role == "Doctor");
                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy bác sĩ với ID {scheduleDTO.DoctorId}");
                }

                // Kiểm tra xem đã có lịch cho bác sĩ vào ngày đó chưa
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var existingSchedule = await scheduleRepository.GetAsync(
                    s => s.DoctorId == scheduleDTO.DoctorId && s.WorkDate == scheduleDTO.WorkDate
                );

                if (existingSchedule != null)
                {
                    throw new InvalidOperationException($"Bác sĩ đã có lịch làm việc cho ngày {scheduleDTO.WorkDate}");
                }

                // Tạo lịch mới với các slot cố định
                var newSchedule = new DoctorSchedule
                {
                    DoctorId = scheduleDTO.DoctorId,
                    WorkDate = scheduleDTO.WorkDate,
                    StartTime = WORK_START_TIME,
                    EndTime = WORK_END_TIME,
                    SlotDuration = SLOT_DURATION,
                    Status = "Available",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await scheduleRepository.AddAsync(newSchedule);
                await _unitOfWork.SaveChangesAsync();

                var createdSchedule = await scheduleRepository.GetAsync(
                    s => s.ScheduleId == newSchedule.ScheduleId,
                    includeProperties: "Doctor"
                );

                var result = _mapper.Map<DoctorScheduleDTO>(createdSchedule);
                result.AvailableSlots = await CalculateAvailableSlotsAsync(newSchedule.ScheduleId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo lịch làm việc cho bác sĩ");
                throw;
            }
        }

        public async Task<DoctorScheduleDTO> UpdateScheduleAsync(int scheduleId, UpdateDoctorScheduleDTO scheduleDTO)
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedule = await scheduleRepository.GetAsync(s => s.ScheduleId == scheduleId);

                if (schedule == null)
                {
                    throw new KeyNotFoundException($"Schedule with ID {scheduleId} not found");
                }

                // Kiểm tra xem có cuộc hẹn nào đã được đặt cho lịch này không
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var existingAppointments = await appointmentRepository.FindAsync(a => a.ScheduleId == scheduleId);

                if (existingAppointments.Any() && (scheduleDTO.StartTime.HasValue || scheduleDTO.EndTime.HasValue || scheduleDTO.SlotDuration.HasValue))
                {
                    throw new InvalidOperationException("Cannot modify time slots for a schedule that has appointments");
                }

                // Cập nhật thông tin
                if (scheduleDTO.StartTime.HasValue)
                {
                    schedule.StartTime = scheduleDTO.StartTime.Value;
                }

                if (scheduleDTO.EndTime.HasValue)
                {
                    schedule.EndTime = scheduleDTO.EndTime.Value;
                }

                if (scheduleDTO.SlotDuration.HasValue)
                {
                    schedule.SlotDuration = scheduleDTO.SlotDuration.Value;
                }

                if (!string.IsNullOrEmpty(scheduleDTO.Status))
                {
                    schedule.Status = scheduleDTO.Status;
                }

                schedule.UpdatedAt = DateTime.UtcNow;

                scheduleRepository.Update(schedule);
                await _unitOfWork.SaveChangesAsync();

                var updatedSchedule = await scheduleRepository.GetAsync(
                    s => s.ScheduleId == scheduleId,
                    includeProperties: "Doctor"
                );

                var result = _mapper.Map<DoctorScheduleDTO>(updatedSchedule);
                result.AvailableSlots = await CalculateAvailableSlotsAsync(scheduleId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating schedule {scheduleId}");
                throw;
            }
        }

        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedule = await scheduleRepository.GetAsync(s => s.ScheduleId == scheduleId);

                if (schedule == null)
                {
                    throw new KeyNotFoundException($"Schedule with ID {scheduleId} not found");
                }

                // Kiểm tra xem có cuộc hẹn nào đã được đặt cho lịch này không
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var existingAppointments = await appointmentRepository.FindAsync(a => a.ScheduleId == scheduleId);

                if (existingAppointments.Any())
                {
                    throw new InvalidOperationException("Cannot delete a schedule that has appointments");
                }

                scheduleRepository.Delete(schedule);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting schedule {scheduleId}");
                throw;
            }
        }

        public async Task<IEnumerable<TimeSlotDTO>> GetAvailableSlotsAsync(int scheduleId)
        {
            try
            {
                return await CalculateAvailableSlotsAsync(scheduleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting available slots for schedule {scheduleId}");
                throw;
            }
        }

        public async Task<bool> IsSlotAvailableAsync(int scheduleId, TimeOnly slotTime)
        {
            try
            {
                var slots = await CalculateAvailableSlotsAsync(scheduleId);
                var slot = slots.FirstOrDefault(s => s.SlotTime == slotTime);

                return slot != null && slot.IsAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if slot {slotTime} is available for schedule {scheduleId}");
                throw;
            }
        }

        public async Task<List<TimeSlotDTO>> CalculateAvailableSlotsAsync(int scheduleId)
        {
            var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
            var schedule = await scheduleRepository.GetAsync(s => s.ScheduleId == scheduleId);

            if (schedule == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy lịch làm việc với ID {scheduleId}");
            }

            var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
            var appointments = await appointmentRepository.FindAsync(
                a => a.ScheduleId == scheduleId
            );

            var slots = new List<TimeSlotDTO>();
            var currentTime = schedule.StartTime;

            while (AddMinutes(currentTime, SLOT_DURATION) <= schedule.EndTime)
            {
                var appointment = appointments.FirstOrDefault(a => a.SlotTime == currentTime);
                var isAvailable = appointment == null || appointment.Status == "Cancelled";

                slots.Add(new TimeSlotDTO
                {
                    SlotTime = currentTime,
                    IsAvailable = isAvailable,
                    IsCancelled = appointment?.Status == "Cancelled",
                    AppointmentId = appointment?.AppointmentId
                });

                currentTime = AddMinutes(currentTime, SLOT_DURATION);
            }

            return slots;
        }

        private TimeOnly AddMinutes(TimeOnly time, int minutes)
        {
            return time.AddMinutes(minutes);
        }
    }
}
