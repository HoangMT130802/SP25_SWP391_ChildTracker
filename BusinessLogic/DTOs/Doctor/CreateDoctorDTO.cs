using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Doctor
{
    public class CreateDoctorDTO
    {
        // Thông tin User
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // Thông tin DoctorProfile
        public string Specialization { get; set; }
        public string Qualification { get; set; }
        public int Experience { get; set; }
        public string LicenseNumber { get; set; }
        public string Biography { get; set; }
    }
}
