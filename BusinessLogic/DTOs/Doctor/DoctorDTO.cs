﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Doctor
{
    public class DoctorDTO
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
        public int Experience { get; set; }
        public string LicenseNumber { get; set; }
        public string Biography { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public bool IsVerified { get; set; }
    }

}
