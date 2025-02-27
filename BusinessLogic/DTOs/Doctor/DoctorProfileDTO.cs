using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Doctor
{
    public class DoctorProfileDTO
    {
        public int DoctorProfileId { get; set; }

        public string Specialization { get; set; }

        public string Qualification { get; set; }

        public int Experience { get; set; }

        public string LicenseNumber { get; set; }

        public string Biography { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalRatings { get; set; }

    }
}
