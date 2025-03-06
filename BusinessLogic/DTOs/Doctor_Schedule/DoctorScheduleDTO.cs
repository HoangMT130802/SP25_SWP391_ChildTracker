using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BusinessLogic.Utils;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class DoctorScheduleDTO
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialization { get; set; }
        
        [JsonConverter(typeof(DateTimeConverters.DateOnlyJsonConverter))]
        public DateOnly WorkDate { get; set; }
        
        [JsonConverter(typeof(DateTimeConverters.TimeOnlyJsonConverter))]
        public TimeOnly StartTime { get; set; }
        
        [JsonConverter(typeof(DateTimeConverters.TimeOnlyJsonConverter))]
        public TimeOnly EndTime { get; set; }
        
        public int SlotDuration { get; set; }
        public string Status { get; set; }
        public List<int> SelectedSlotIds { get; set; }
        public List<TimeSlotDTO> AvailableSlots { get; set; } = new List<TimeSlotDTO>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 