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
        private const int SLOT_DURATION = 45;
        
        private static readonly List<TimeSlot> DEFAULT_SLOTS = new List<TimeSlot>
        {
            new TimeSlot { SlotId = 1, StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(8, 45) },
            new TimeSlot { SlotId = 2, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 45) },
            new TimeSlot { SlotId = 3, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(10, 45) },
            new TimeSlot { SlotId = 4, StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(11, 45) },
            new TimeSlot { SlotId = 5, StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(13, 45) },
            new TimeSlot { SlotId = 6, StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(14, 45) },
            new TimeSlot { SlotId = 7, StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(15, 45) },
            new TimeSlot { SlotId = 8, StartTime = new TimeOnly(16, 0), EndTime = new TimeOnly(16, 45) }
        };

        public DoctorScheduleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<DoctorScheduleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public List<TimeSlotDTO> GetDefaultTimeSlots()
        {
            return DEFAULT_SLOTS.Select(slot => new TimeSlotDTO
            {
                SlotId = slot.SlotId,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                SlotTime = slot.StartTime,
                IsAvailable = true,
                IsCancelled = false,
                AppointmentId = null,
                Status = "Available"
            }).ToList();
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
                    scheduleDTO.AvailableSlots = (await CalculateAvailableSlotsAsync(scheduleDTO.ScheduleId)).ToList();
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
                foreach (var scheduleDTO in scheduleDTOs)
                {
                    scheduleDTO.AvailableSlots = (await CalculateAvailableSlotsAsync(scheduleDTO.ScheduleId)).ToList();
                }

                return scheduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch làm việc của bác sĩ {doctorId}");
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
                foreach (var scheduleDTO in scheduleDTOs)
                {
                    scheduleDTO.AvailableSlots = (await CalculateAvailableSlotsAsync(scheduleDTO.ScheduleId)).ToList();
                }

                return scheduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch làm việc của bác sĩ {doctorId} từ {startDate} đến {endDate}");
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
                    throw new KeyNotFoundException($"Không tìm thấy lịch làm việc với ID {scheduleId}");
                }

                var scheduleDTO = _mapper.Map<DoctorScheduleDTO>(schedule);
                scheduleDTO.AvailableSlots = (await CalculateAvailableSlotsAsync(scheduleId)).ToList();

                return scheduleDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch làm việc {scheduleId}");
                throw;
            }
        }

        public async Task<DoctorScheduleDTO> CreateScheduleAsync(CreateDoctorScheduleDTO scheduleDTO)
        {
            try
            {
                // Parse ngày làm việc từ string
                if (!DateOnly.TryParseExact(scheduleDTO.WorkDate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateOnly workDate))
                {
                    throw new InvalidOperationException("Ngày làm việc không đúng định dạng (yyyy-MM-dd)");
                }

                // Kiểm tra ngày tạo lịch phải sau ngày hiện tại 3 ngày
                var minAllowedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3));
                if (workDate < minAllowedDate)
                {
                    throw new InvalidOperationException("Chỉ được tạo lịch trước 3 ngày");
                }

                // Kiểm tra ngày làm việc phải là thứ 2 đến thứ 6
                var dayOfWeek = workDate.DayOfWeek;
                if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                {
                    throw new InvalidOperationException("Chỉ được tạo lịch từ thứ 2 đến thứ 6");
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
                    s => s.DoctorId == scheduleDTO.DoctorId && s.WorkDate == workDate
                );

                if (existingSchedule != null)
                {
                    throw new InvalidOperationException($"Bác sĩ đã có lịch làm việc cho ngày {workDate}");
                }

                // Kiểm tra số lượng slot được chọn
                if (scheduleDTO.SelectedSlotIds == null || scheduleDTO.SelectedSlotIds.Count < 6)
                {
                    throw new InvalidOperationException("Phải chọn ít nhất 6 slot làm việc");
                }

                // Kiểm tra slot ID hợp lệ và sắp xếp theo thứ tự
                var selectedSlots = scheduleDTO.SelectedSlotIds.OrderBy(x => x).ToList();
                foreach (var slotId in selectedSlots)
                {
                    if (!DEFAULT_SLOTS.Any(s => s.SlotId == slotId))
                    {
                        throw new InvalidOperationException($"Slot ID {slotId} không hợp lệ");
                    }
                }

                // Tạo lịch mới
                var newSchedule = new DoctorSchedule
                {
                    DoctorId = scheduleDTO.DoctorId,
                    WorkDate = workDate,
                    StartTime = DEFAULT_SLOTS[selectedSlots.First() - 1].StartTime,
                    EndTime = DEFAULT_SLOTS[selectedSlots.Last() - 1].EndTime,
                    SlotDuration = SLOT_DURATION,
                    Status = "Available",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await scheduleRepository.AddAsync(newSchedule);
                await _unitOfWork.SaveChangesAsync();

                var result = _mapper.Map<DoctorScheduleDTO>(newSchedule);
                result.SelectedSlotIds = selectedSlots;
                result.AvailableSlots = GetSelectedTimeSlots(selectedSlots);

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
                    throw new KeyNotFoundException($"Không tìm thấy lịch làm việc với ID {scheduleId}");
                }

                // Kiểm tra xem có cuộc hẹn nào đã được đặt cho lịch này không
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var existingAppointments = await appointmentRepository.FindAsync(a => a.ScheduleId == scheduleId);

                if (existingAppointments.Any() && (scheduleDTO.StartTime.HasValue || scheduleDTO.EndTime.HasValue || scheduleDTO.SlotDuration.HasValue))
                {
                    throw new InvalidOperationException("Không thể thay đổi thời gian của lịch đã có cuộc hẹn");
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
                result.AvailableSlots = (await CalculateAvailableSlotsAsync(scheduleId)).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật lịch làm việc {scheduleId}");
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
                    throw new KeyNotFoundException($"Không tìm thấy lịch làm việc với ID {scheduleId}");
                }

                // Kiểm tra xem có cuộc hẹn nào đã được đặt cho lịch này không
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var existingAppointments = await appointmentRepository.FindAsync(a => a.ScheduleId == scheduleId);

                if (existingAppointments.Any())
                {
                    throw new InvalidOperationException("Không thể xóa lịch làm việc đã có cuộc hẹn");
                }

                scheduleRepository.Delete(schedule);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi xóa lịch làm việc {scheduleId}");
                throw;
            }
        }

        public async Task<List<TimeSlotDTO>> CalculateAvailableSlotsAsync(int scheduleId)
        {
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedule = await scheduleRepository.GetAsync(s => s.ScheduleId == scheduleId);

                if (schedule == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy lịch làm việc với ID {scheduleId}");
                }

                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointments = await appointmentRepository.FindAsync(a => a.ScheduleId == scheduleId);

                var slots = new List<TimeSlotDTO>();
                var currentTime = schedule.StartTime;

                while (AddMinutes(currentTime, SLOT_DURATION) <= schedule.EndTime)
                {
                    var appointment = appointments.FirstOrDefault(a => a.SlotTime == currentTime);
                    var isAvailable = schedule.Status == "Available" && 
                                    (appointment == null || appointment.Status == "Cancelled");

                    slots.Add(new TimeSlotDTO
                    {
                        SlotId = GetSlotIdByTime(currentTime),
                        StartTime = currentTime,
                        EndTime = AddMinutes(currentTime, SLOT_DURATION),
                        SlotTime = currentTime,
                        IsAvailable = isAvailable,
                        IsCancelled = appointment?.Status == "Cancelled",
                        AppointmentId = appointment?.AppointmentId,
                        Status = appointment?.Status ?? "Available"
                    });

                    currentTime = AddMinutes(currentTime, SLOT_DURATION);
                }

                return slots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tính toán các slot cho lịch {scheduleId}");
                throw;
            }
        }

        private int GetSlotIdByTime(TimeOnly time)
        {
            var slot = DEFAULT_SLOTS.FirstOrDefault(s => s.StartTime == time);
            return slot?.SlotId ?? 0;
        }

        public async Task<IEnumerable<TimeSlotDTO>> GetAvailableSlotsAsync(int scheduleId)
        {
            return await CalculateAvailableSlotsAsync(scheduleId);
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
                _logger.LogError(ex, $"Lỗi khi kiểm tra slot {slotTime} của lịch {scheduleId}");
                throw;
            }
        }

        private TimeOnly AddMinutes(TimeOnly time, int minutes)
        {
            return time.AddMinutes(minutes);
        }

        public async Task<IEnumerable<DoctorScheduleDTO>> GetDoctorSchedulesByWeekAsync(int doctorId, DateOnly weekStart)
        {
            try
            {
                var weekEnd = weekStart.AddDays(4); // Thứ 2 đến thứ 6
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedules = await scheduleRepository.FindAsync(
                    s => s.DoctorId == doctorId && 
                         s.WorkDate >= weekStart && 
                         s.WorkDate <= weekEnd,
                    includeProperties: "Doctor"
                );

                var scheduleDTOs = _mapper.Map<IEnumerable<DoctorScheduleDTO>>(schedules);
                foreach (var scheduleDTO in scheduleDTOs)
                {
                    var schedule = schedules.First(s => s.ScheduleId == scheduleDTO.ScheduleId);
                    var selectedSlotIds = GetSelectedSlotIdsFromSchedule(schedule);
                    scheduleDTO.SelectedSlotIds = selectedSlotIds;
                    scheduleDTO.AvailableSlots = (await GetAvailableSlotsAsync(scheduleDTO.ScheduleId)).ToList();
                }

                return scheduleDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy lịch làm việc của bác sĩ {doctorId}");
                throw;
            }
        }

        private List<int> GetSelectedSlotIdsFromSchedule(DoctorSchedule schedule)
        {
            return DEFAULT_SLOTS
                .Where(slot => slot.StartTime >= schedule.StartTime && slot.EndTime <= schedule.EndTime)
                .Select(slot => slot.SlotId)
                .ToList();
        }

        private List<TimeSlotDTO> GetSelectedTimeSlots(List<int> selectedSlotIds)
        {
            return DEFAULT_SLOTS
                .Where(slot => selectedSlotIds.Contains(slot.SlotId))
                .Select(slot => new TimeSlotDTO
                {
                    SlotId = slot.SlotId,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    SlotTime = slot.StartTime,
                    IsAvailable = true,
                    IsCancelled = false,
                    AppointmentId = null,
                    Status = "Available"
                })
                .ToList();
        }
    }

    public class TimeSlot
    {
        public int SlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
