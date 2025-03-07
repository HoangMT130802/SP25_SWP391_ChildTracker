using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.UserMembership
{
    public class MembershipDto
    {
        public int MembershipId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } // Số ngày hiệu lực
        public string Price { get; set; } 
        public int MaxChildren { get; set; }
        public int MaxConsultations { get; set; }
    }
}
