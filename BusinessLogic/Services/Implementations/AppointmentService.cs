using AutoMapper;
using BusinessLogic.DTOs.Appointment;
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
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentService> _logger;
        private readonly IDoctorScheduleService _scheduleService;

        public AppointmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AppointmentService> logger,
            IDoctorScheduleService scheduleService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _scheduleService = scheduleService;
        }

        public async Task<IEnumerable<AppointmentDTO>> GetAllAppointmentsAsync()
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointments = await appointmentRepository.GetAllAsync(
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);

                // Thêm thông tin ngày từ lịch bác sĩ
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all appointments");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDTO>> GetUserAppointmentsAsync(int userId)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointments = await appointmentRepository.FindAsync(
                    a => a.UserId == userId,
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);

                // Thêm thông tin ngày từ lịch bác sĩ
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDTO>> GetDoctorAppointmentsAsync(int doctorId)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointments = await appointmentRepository.FindAsync(
                    a => a.DoctorId == doctorId,
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);

                // Thêm thông tin ngày từ lịch bác sĩ
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for doctor {doctorId}");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDTO>> GetChildAppointmentsAsync(int childId)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointments = await appointmentRepository.FindAsync(
                    a => a.ChildId == childId,
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);

                // Thêm thông tin ngày từ lịch bác sĩ
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for child {childId}");
                throw;
            }
        }

        public async Task<AppointmentDTO> GetAppointmentByIdAsync(int appointmentId)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointmentId,
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");
                }

                var appointmentDTO = _mapper.Map<AppointmentDTO>(appointment);

                // Thêm thông tin ngày từ lịch bác sĩ
                if (appointment.Schedule != null)
                {
                    appointmentDTO.AppointmentDate = appointment.Schedule.WorkDate;
                }

                return appointmentDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointment {appointmentId}");
                throw;
            }
        }

        public async Task<AppointmentDTO> CreateAppointmentAsync(CreateAppointmentDTO appointmentDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedule = await scheduleRepository.GetAsync(s => s.ScheduleId == appointmentDTO.ScheduleId);

                if (schedule == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy lịch làm việc");
                }

                // Kiểm tra xem user đã từng hủy lịch trong ngày này chưa
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var cancelledToday = await appointmentRepository.FindAsync(
                    a => a.UserId == appointmentDTO.UserId &&
                         a.Schedule.WorkDate == schedule.WorkDate &&
                         a.Status == "Cancelled"
                );

                if (cancelledToday.Any())
                {
                    throw new InvalidOperationException("Bạn đã hủy lịch hẹn trong ngày này và không thể đặt lại");
                }

                // Kiểm tra slot có sẵn
               /* var slots = await _scheduleService.CalculateAvailableSlotsAsync(appointmentDTO.ScheduleId);
                var selectedSlot = slots.FirstOrDefault(s => s.SlotTime == appointmentDTO.SlotTime);

                if (selectedSlot == null || !selectedSlot.IsAvailable)
                {
                    throw new InvalidOperationException("Slot này không khả dụng");
                }*/

                // Tạo link Google Meet
                string meetingLink = await CreateGoogleMeetLinkAsync();

                // Tạo cuộc hẹn mới
                var appointment = new Appointment
                {
                    ScheduleId = appointmentDTO.ScheduleId,
                    UserId = appointmentDTO.UserId,
                    DoctorId = schedule.DoctorId,
                    ChildId = appointmentDTO.ChildId,
                    SlotTime = appointmentDTO.SlotTime,
                    Status = "Pending",
                    MeetingLink = meetingLink,
                    Description = appointmentDTO.Description,
                    CreatedAt = DateTime.UtcNow
                };

                await appointmentRepository.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // Lấy thông tin đầy đủ của cuộc hẹn
                var createdAppointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointment.AppointmentId,
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var result = _mapper.Map<AppointmentDTO>(createdAppointment);
                if (createdAppointment.Schedule != null)
                {
                    result.AppointmentDate = createdAppointment.Schedule.WorkDate;
                }

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo cuộc hẹn");
                throw;
            }
        }

        public async Task<AppointmentDTO> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentDTO appointmentDTO)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointment = await appointmentRepository.GetAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found");
                }

                // Cập nhật thông tin
                if (!string.IsNullOrEmpty(appointmentDTO.Status))
                {
                    appointment.Status = appointmentDTO.Status;
                }

                if (!string.IsNullOrEmpty(appointmentDTO.MeetingLink))
                {
                    appointment.MeetingLink = appointmentDTO.MeetingLink;
                }

                if (!string.IsNullOrEmpty(appointmentDTO.Note))
                {
                    appointment.Note = appointmentDTO.Note;
                }

                appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                // Lấy cuộc hẹn đã cập nhật với thông tin đầy đủ
                var updatedAppointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointmentId,
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var result = _mapper.Map<AppointmentDTO>(updatedAppointment);

                // Thêm thông tin ngày từ lịch bác sĩ
                if (updatedAppointment.Schedule != null)
                {
                    result.AppointmentDate = updatedAppointment.Schedule.WorkDate;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating appointment {appointmentId}");
                throw;
            }
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId, int userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointmentId && 
                         (a.UserId == userId || a.DoctorId == userId),
                    includeProperties: "Schedule"
                );

                if (appointment == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy cuộc hẹn hoặc bạn không có quyền hủy");
                }

                // Kiểm tra trạng thái cuộc hẹn
                if (appointment.Status == "Cancelled")
                {
                    throw new InvalidOperationException("Cuộc hẹn đã được hủy trước đó");
                }

                if (appointment.Status == "Completed")
                {
                    throw new InvalidOperationException("Không thể hủy cuộc hẹn đã hoàn thành");
                }

                // Kiểm tra thời gian hủy (có thể thêm logic kiểm tra thời gian hủy trước cuộc hẹn)
                
                appointment.Status = "Cancelled";
                appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi khi hủy cuộc hẹn {appointmentId}");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDTO>> GetAppointmentsByDateRangeAsync(int userId, DateOnly startDate, DateOnly endDate)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();

                // Lấy tất cả lịch trong khoảng ngày
                var schedules = await scheduleRepository.FindAsync(
                    s => s.WorkDate >= startDate && s.WorkDate <= endDate
                );

                var scheduleIds = schedules.Select(s => s.ScheduleId).ToList();

                // Lấy các cuộc hẹn của người dùng trong các lịch đó
                var appointments = await appointmentRepository.FindAsync(
                    a => (a.UserId == userId || a.DoctorId == userId) && scheduleIds.Contains(a.ScheduleId),
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);

                // Thêm thông tin ngày từ lịch bác sĩ
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for user {userId} between {startDate} and {endDate}");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDTO>> GetDoctorAppointmentsByDateAsync(int doctorId, DateOnly date)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();

                // Lấy tất cả lịch của bác sĩ trong ngày
                var schedules = await scheduleRepository.FindAsync(
                    s => s.DoctorId == doctorId && s.WorkDate == date
                );

                var scheduleIds = schedules.Select(s => s.ScheduleId).ToList();

                // Lấy các cuộc hẹn trong các lịch đó
                var appointments = await appointmentRepository.FindAsync(
                    a => scheduleIds.Contains(a.ScheduleId),
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);

                // Thêm thông tin ngày từ lịch bác sĩ
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting appointments for doctor {doctorId} on {date}");
                throw;
            }
        }

        private async Task<string> CreateGoogleMeetLinkAsync()
        {
            // TODO: Implement Google Meet API integration
            return $"https://meet.google.com/{Guid.NewGuid().ToString("N").Substring(0, 12)}";
        }
    }
}