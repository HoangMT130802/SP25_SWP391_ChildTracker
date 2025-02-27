using BusinessLogic.DTOs.Doctor_Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<IEnumerable<DoctorScheduleDTO>> GetAllSchedulesAsync();
       
    }
}
