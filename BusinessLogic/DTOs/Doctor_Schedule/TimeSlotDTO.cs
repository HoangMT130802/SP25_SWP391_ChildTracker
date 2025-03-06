using System;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class TimeSlotDTO
    {
        public int SlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public TimeOnly SlotTime { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsCancelled { get; set; }
        public int? AppointmentId { get; set; }
        public string Status { get; set; }
    }
} 