using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Appointment
{
    public class CreateAppointmentDTO
    {
        public int ScheduleId { get; set; }
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public int ChildId { get; set; }
        public TimeOnly SlotTime { get; set; }
        public string Description { get; set; }
    }
}
