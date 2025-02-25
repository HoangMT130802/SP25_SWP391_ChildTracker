using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Children
{
    public class ChildrenDTO
    {
        public int child_id { get; set; }
        public string FullName { get; set; }
        public string ParentName { get; set; }
        public String ParentNumber { get; set; }
        public DateTime birth_date { get; set; }
        public string gender { get; set; }
        public string BloodType { get; set; }
        public string AllergiesNotes { get; set; }
        public string MedicalHistory { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
