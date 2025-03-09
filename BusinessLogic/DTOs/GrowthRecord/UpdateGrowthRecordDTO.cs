using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.GrowthRecord
{
    public class UpdateGrowthRecordDTO
    {
        public decimal Height { get; set; }
        public decimal Weight { get; set; }
        public decimal HeadCircumference { get; set; }
        public string Note { get; set; }
    }
}
