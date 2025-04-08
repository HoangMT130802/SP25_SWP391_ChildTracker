using BusinessLogic.Mappers;
using System;

namespace BusinessLogic.DTOs.Appointment
{
    public class AppointmentDTO 
    {
        public int AppointmentId { get; set; }
        public int ScheduleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public string SlotTime { get; set; }
        public string Status { get; set; }
        public TimeOnly AppointmentTime { get; set; }
        public string MeetingLink { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 