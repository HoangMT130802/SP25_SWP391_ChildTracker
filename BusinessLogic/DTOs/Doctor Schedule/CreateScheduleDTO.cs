using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class CreateDoctorScheduleDTO
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateOnly WorkDate { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        [Required]
        [Range(15, 120)]
        public int SlotDuration { get; set; }
    }
}
