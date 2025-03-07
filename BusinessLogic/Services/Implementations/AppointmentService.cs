using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.DTOs.Appointment;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<AppointmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<AppointmentDTO>> GetUserAppointmentsAsync(int userId)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointments = await appointmentRepository.FindAsync(
                    a => a.UserId == userId,
                    includeProperties: "User,Child,Schedule,Schedule.Doctor"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                        dto.DoctorId = appointment.Schedule.DoctorId;
                        dto.DoctorName = appointment.Schedule.Doctor?.FullName;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách lịch hẹn của người dùng {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<AppointmentDTO>> GetDoctorAppointmentsAsync(int doctorId)
        {
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointments = await appointmentRepository.FindAsync(
                    a => a.Schedule.DoctorId == doctorId,
                    includeProperties: "User,Child,Schedule,Schedule.Doctor"
                );

                var appointmentDTOs = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
                foreach (var dto in appointmentDTOs)
                {
                    var appointment = appointments.FirstOrDefault(a => a.AppointmentId == dto.AppointmentId);
                    if (appointment?.Schedule != null)
                    {
                        dto.AppointmentDate = appointment.Schedule.WorkDate;
                        dto.DoctorId = appointment.Schedule.DoctorId;
                        dto.DoctorName = appointment.Schedule.Doctor?.FullName;
                    }
                }

                return appointmentDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách lịch hẹn của bác sĩ {doctorId}");
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
                    includeProperties: "User,Child,Schedule,Schedule.Doctor"
                );

                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy lịch hẹn với ID {appointmentId}");
                }

                var appointmentDTO = _mapper.Map<AppointmentDTO>(appointment);
                if (appointment.Schedule != null)
                {
                    appointmentDTO.AppointmentDate = appointment.Schedule.WorkDate;
                    appointmentDTO.DoctorId = appointment.Schedule.DoctorId;
                    appointmentDTO.DoctorName = appointment.Schedule.Doctor?.FullName;
                }

                return appointmentDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy thông tin lịch hẹn {appointmentId}");
                throw;
            }
        }

        public async Task<AppointmentDTO> CreateAppointmentAsync(CreateAppointmentDTO appointmentDTO)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Lấy thông tin lịch bác sĩ
                var scheduleRepository = _unitOfWork.GetRepository<DoctorSchedule>();
                var schedule = await scheduleRepository.GetAsync(
                    s => s.ScheduleId == appointmentDTO.ScheduleId,
                    includeProperties: "Doctor"
                );

                if (schedule == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy lịch làm việc");
                }

                // Kiểm tra slot có tồn tại trong selectedSlots không
                var selectedSlots = schedule.SelectedSlots.Split(',').Select(int.Parse).ToList();
                var slotId = int.Parse(appointmentDTO.SlotTime);
                if (!selectedSlots.Contains(slotId))
                {
                    throw new InvalidOperationException("Slot không tồn tại trong lịch làm việc này");
                }

                // Kiểm tra thời gian slot có hợp lệ không
                var now = DateTime.Now;
                if (schedule.WorkDate < DateOnly.FromDateTime(now))
                {
                    throw new InvalidOperationException("Không thể đặt lịch cho ngày đã qua");
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

                // Kiểm tra slot có trống không
                var existingAppointment = await appointmentRepository.GetAsync(
                    a => a.ScheduleId == appointmentDTO.ScheduleId &&
                         a.SlotTime == appointmentDTO.SlotTime &&
                         a.Status != "Cancelled"
                );

                if (existingAppointment != null)
                {
                    throw new InvalidOperationException("Slot này đã được đặt");
                }

                // Tạo link Google Meet
                string meetingLink = $"https://meet.google.com/{Guid.NewGuid().ToString("N").Substring(0, 12)}";

                // Tạo cuộc hẹn mới
                var appointment = new Appointment
                {
                    ScheduleId = appointmentDTO.ScheduleId,
                    UserId = appointmentDTO.UserId,
                    ChildId = appointmentDTO.ChildId,
                    SlotTime = appointmentDTO.SlotTime,
                    Status = "Pending",
                    MeetingLink = meetingLink,
                    Description = appointmentDTO.Description ?? string.Empty,
                    Note = appointmentDTO.Note ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await appointmentRepository.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();

                // Lấy thông tin đầy đủ của cuộc hẹn
                var createdAppointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointment.AppointmentId,
                    includeProperties: "User,Child,Schedule,Schedule.Doctor"
                );

                var result = _mapper.Map<AppointmentDTO>(createdAppointment);
                if (createdAppointment.Schedule != null)
                {
                    result.AppointmentDate = createdAppointment.Schedule.WorkDate;
                    result.DoctorId = createdAppointment.Schedule.DoctorId;
                    result.DoctorName = createdAppointment.Schedule.Doctor?.FullName;
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

        public async Task<bool> CancelAppointmentAsync(int appointmentId, int userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var appointmentRepository = _unitOfWork.GetRepository<Appointment>();
                var appointment = await appointmentRepository.GetAsync(
                    a => a.AppointmentId == appointmentId && 
                         (a.UserId == userId || a.Schedule.DoctorId == userId),
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

                // Kiểm tra thời gian hủy
                var now = DateTime.Now;
                if (appointment.Schedule.WorkDate < DateOnly.FromDateTime(now))
                {
                    throw new InvalidOperationException("Không thể hủy cuộc hẹn đã qua");
                }

                // Kiểm tra số lần hủy trong ngày
                var cancelledToday = await appointmentRepository.FindAsync(
                    a => a.UserId == userId &&
                         a.Schedule.WorkDate == appointment.Schedule.WorkDate &&
                         a.Status == "Cancelled"
                );

                if (cancelledToday.Any())
                {
                    throw new InvalidOperationException("Bạn đã hủy một cuộc hẹn khác trong ngày này");
                }
                
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
    }
} 