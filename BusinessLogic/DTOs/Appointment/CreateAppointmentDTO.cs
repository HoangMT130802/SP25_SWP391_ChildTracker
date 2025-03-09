using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Appointment
{
    public class CreateAppointmentDTO
    {
        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        public string SlotTime { get; set; }

        public string Description { get; set; }

        public string Note { get; set; } = string.Empty;
    }
} 