using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.GrowthRecord
{
    public class GrowthAssessmentDTO
    {
        public decimal ZScoreHeight { get; set; }
        public decimal ZScoreWeight { get; set; }
        public decimal ZScoreBMI { get; set; }
        public decimal ZScoreHeadCircumference { get; set; }

        public string HeightStatus { get; set; } // Bình thường/Thấp còi độ 1/Thấp còi độ 2...
        public string WeightStatus { get; set; } // Bình thường/Suy dinh dưỡng/Thừa cân...
        public string BMIStatus { get; set; } // Bình thường/Gầy/Béo phì độ 1/Béo phì độ 2...
        public string HeadCircumferenceStatus { get; set; }

        public string Recommendations { get; set; } // Lời khuyên tổng hợp
    }
}
