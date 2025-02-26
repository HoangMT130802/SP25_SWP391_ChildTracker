﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Children
{
    public class CreateChildDTO
    {
        public string FullName { get; set; }
        public string ParentName { get; set; }
        public string ParentNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string AllergiesNotes { get; set; }
        public string MedicalHistory { get; set; }
    }
}
