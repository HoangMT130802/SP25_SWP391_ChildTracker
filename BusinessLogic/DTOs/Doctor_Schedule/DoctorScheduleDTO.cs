using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class DoctorScheduleDTO
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        public DateOnly WorkDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int SlotDuration { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TimeSlotDTO> AvailableSlots { get; set; } = new List<TimeSlotDTO>();
    }
} 