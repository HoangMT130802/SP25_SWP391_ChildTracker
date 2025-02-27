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

                // Tính toán các slot có sẵn cho mỗi lịch
                foreach (var scheduleDTO in scheduleDTOs)
                {
                    scheduleDTO.AvailableSlots = await CalculateAvailableSlotsAsync(scheduleDTO.ScheduleId);
                }

                return scheduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all doctor schedules");
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
                // Kiểm tra bác sĩ có tồn tại không
                var userRepository = _unitOfWork.GetRepository<User>();
                var doctor = await userRepository.GetAsync(u => u.UserId == scheduleDTO.DoctorId && u.Role == "Doctor");

                if (doctor == null)
                {
                    throw new KeyNotFoundException($"Doctor with ID {scheduleDTO.DoctorId} not found");
                }

                // Kiểm tra thời gian hợp lệ
                if (scheduleDTO.EndTime <= scheduleDTO.StartTime)
                {
                    throw new ArgumentException("End time must be after start time");
                }

                // Kiểm tra xem đã có lịch cho bác sĩ vào ngày đó chưa
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var existingSchedule = await scheduleRepository.GetAsync(
                    s => s.DoctorId == scheduleDTO.DoctorId && s.WorkDate == scheduleDTO.WorkDate
                );

                if (existingSchedule != null)
                {
                    throw new InvalidOperationException($"Doctor already has a schedule for {scheduleDTO.WorkDate}");
                }

                // Tạo lịch mới
                var newSchedule = new DoctorSchedule
                {
                    DoctorId = scheduleDTO.DoctorId,
                    WorkDate = scheduleDTO.WorkDate,
                    StartTime = scheduleDTO.StartTime,
                    EndTime = scheduleDTO.EndTime,
                    SlotDuration = scheduleDTO.SlotDuration,
                    Status = "Available",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await scheduleRepository.AddAsync(newSchedule);
                await _unitOfWork.SaveChangesAsync();

                // Lấy lịch đã tạo với thông tin bác sĩ
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
                _logger.LogError(ex, "Error creating doctor schedule");
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

        private async Task<List<TimeSlotDTO>> CalculateAvailableSlotsAsync(int scheduleId)
        {
            var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
            var schedule = await scheduleRepository.GetAsync(s => s.ScheduleId == scheduleId);

            if (schedule == null)
            {
                throw new KeyNotFoundException($"Schedule with ID {scheduleId} not found");
            }

            var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
            var bookedAppointments = await appointmentRepository.FindAsync(
                a => a.ScheduleId == scheduleId && a.Status != "Cancelled"
            );

            var slots = new List<TimeSlotDTO>();
            var currentTime = schedule.StartTime;

            while (AddMinutes(currentTime, schedule.SlotDuration) <= schedule.EndTime)
            {
                var isAvailable = schedule.Status == "Available" &&
                                 !bookedAppointments.Any(a => a.SlotTime == currentTime);

                slots.Add(new TimeSlotDTO
                {
                    SlotTime = currentTime,
                    IsAvailable = isAvailable
                });

                currentTime = AddMinutes(currentTime, schedule.SlotDuration);
            }

            return slots;
        }

        private TimeOnly AddMinutes(TimeOnly time, int minutes)
        {
            return time.AddMinutes(minutes);
        }
    }
}
