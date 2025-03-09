using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.User;

namespace BusinessLogic.DTOs.Doctor
{
    public class DoctorProfileDTO : BaseUserDTO
    {
        public string Specialization { get; set; }
        public string Qualification { get; set; }
        public string LicenseNumber { get; set; }
        public string WorkPlace { get; set; }
        public string Experience { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public int ConsultationCount { get; set; }
        public bool IsAvailable { get; set; }
        public string WorkingHours { get; set; }
    }
}
