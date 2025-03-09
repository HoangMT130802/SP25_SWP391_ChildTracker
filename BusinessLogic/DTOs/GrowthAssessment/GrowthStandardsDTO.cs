using DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.GrowthAssessment
{
    public class GrowthStandardsDTO
    {
        public GrowthStandard Height { get; set; }
        public GrowthStandard Weight { get; set; }
        public GrowthStandard BMI { get; set; }
        public GrowthStandard HeadCircumference { get; set; }
    }
}
