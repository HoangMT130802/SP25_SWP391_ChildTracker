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

        public AppointmentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AppointmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
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
            try
            {
                // Kiểm tra xem lịch bác sĩ có tồn tại không
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedule = await scheduleRepository.GetAsync(s => s.ScheduleId == appointmentDTO.ScheduleId);

                if (schedule == null)
                {
                    throw new KeyNotFoundException($"Schedule with ID {appointmentDTO.ScheduleId} not found");
                }

                // Kiểm tra xem trẻ có tồn tại không
                var childRepository = _unitOfWork.GetRepository<Child>();
                var child = await childRepository.GetAsync(c => c.ChildId == appointmentDTO.ChildId);

                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {appointmentDTO.ChildId} not found");
                }

                // Kiểm tra xem slot có sẵn không
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var existingAppointment = await appointmentRepository.GetAsync(
                    a => a.ScheduleId == appointmentDTO.ScheduleId &&
                         a.SlotTime == appointmentDTO.SlotTime &&
                         a.Status != "Cancelled"
                );

                if (existingAppointment != null)
                {
                    throw new InvalidOperationException("This time slot is already booked");
                }

                // Kiểm tra xem slot có nằm trong khoảng thời gian làm việc của bác sĩ không
                if (appointmentDTO.SlotTime < schedule.StartTime ||
                    appointmentDTO.SlotTime.AddMinutes(schedule.SlotDuration) > schedule.EndTime)
                {
                    throw new ArgumentException("The selected time slot is outside the doctor's working hours");
                }

                // Tạo cuộc hẹn mới
                var appointment = new Appointment
                {
                    ScheduleId = appointmentDTO.ScheduleId,
                    UserId = appointmentDTO.UserId,
                    DoctorId = schedule.DoctorId,
                    ChildId = appointmentDTO.ChildId,
                    SlotTime = appointmentDTO.SlotTime,
                    Status = "Pending",
                    MeetingLink = "", // Sẽ được cập nhật sau khi bác sĩ xác nhận
                    Description = appointmentDTO.Description,
                    Note = "",
                    CreatedAt = DateTime.UtcNow
                };

                await appointmentRepository.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                // Lấy cuộc hẹn đã tạo với thông tin đầy đủ
                var createdAppointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointment.AppointmentId,
                    includeProperties: "User,Doctor,Child,Schedule"
                );

                var result = _mapper.Map<AppointmentDTO>(createdAppointment);

                // Thêm thông tin ngày từ lịch bác sĩ
                if (createdAppointment.Schedule != null)
                {
                    result.AppointmentDate = createdAppointment.Schedule.WorkDate;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
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
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointmentId && (a.UserId == userId || a.DoctorId == userId)
                );

                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found or you don't have permission to cancel it");
                }

                // Kiểm tra xem cuộc hẹn đã hoàn thành chưa
                if (appointment.Status == "Completed")
                {
                    throw new InvalidOperationException("Cannot cancel a completed appointment");
                }

                appointment.Status = "Cancelled";
                appointmentRepository.Update(appointment);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling appointment {appointmentId} for user {userId}");
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
    }
}