using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public TimeOnly SlotTime { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
