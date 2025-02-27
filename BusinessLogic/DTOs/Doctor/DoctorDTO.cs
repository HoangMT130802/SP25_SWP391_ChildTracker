using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Doctor
{
    public class DoctorDTO
    {
        public string FullName { get; set; } 
        public DoctorProfileDTO DoctorProfile { get; set; }
    }

}
