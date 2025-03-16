using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.GrowthStandard
{
    public class GrowthStandardResponse
    {
        public List<GrowthStandardDTO> Standards { get; set; }
        public string Measurement { get; set; }
    }
}
