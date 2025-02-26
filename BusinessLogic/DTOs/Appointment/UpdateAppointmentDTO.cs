using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Appointment
{
    public class UpdateAppointmentDTO
    {
        public string Status { get; set; }
        public string MeetingLink { get; set; }
        public string Note { get; set; }
    }
}
