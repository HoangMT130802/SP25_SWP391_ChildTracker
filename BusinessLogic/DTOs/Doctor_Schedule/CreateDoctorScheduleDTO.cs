using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class CreateDoctorScheduleDTO
    {
        [Required(ErrorMessage = "ID bác sĩ không được để trống")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        public DateOnly WorkDate { get; set; }

        [Required(ErrorMessage = "Giờ bắt đầu không được để trống")]
        public TimeOnly? StartTime { get; set; }

        [Required(ErrorMessage = "Giờ kết thúc không được để trống")]
        public TimeOnly? EndTime { get; set; }

        [Required(ErrorMessage = "Thời lượng slot không được để trống")]
        [Range(15, 120, ErrorMessage = "Thời lượng slot phải từ 15 đến 120 phút")]
        public int? SlotDuration { get; set; }
    }
} 