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
        public DataAccess.Entities.GrowthStandard Height { get; set; }
        public DataAccess.Entities.GrowthStandard Weight { get; set; }
        public DataAccess.Entities.GrowthStandard BMI { get; set; }
        public DataAccess.Entities.GrowthStandard HeadCircumference { get; set; }
    }
}
