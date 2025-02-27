using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class TimeSlotDTO
    {
        public TimeOnly SlotTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
