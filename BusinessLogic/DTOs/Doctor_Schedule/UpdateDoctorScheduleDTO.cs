using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class UpdateDoctorScheduleDTO
    {
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        
        [Range(15, 120, ErrorMessage = "Thời lượng slot phải từ 15 đến 120 phút")]
        public int? SlotDuration { get; set; }
        
        [RegularExpression("^(Available|Unavailable|Cancelled)$", 
            ErrorMessage = "Trạng thái phải là một trong các giá trị: Available, Unavailable, Cancelled")]
        public string Status { get; set; }
    }
} 