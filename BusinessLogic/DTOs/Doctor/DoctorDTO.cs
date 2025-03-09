using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.User;

namespace BusinessLogic.DTOs.Doctor
{
    public class DoctorDTO : BaseUserDTO
    {
        //user
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        //DoctorProfile
        public string Specialization { get; set; }
        public string Qualification { get; set; }
        public string LicenseNumber { get; set; }
        public string WorkPlace { get; set; }
        public string Experience { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public int ConsultationCount { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public bool IsVerified { get; set; }
    }
}
