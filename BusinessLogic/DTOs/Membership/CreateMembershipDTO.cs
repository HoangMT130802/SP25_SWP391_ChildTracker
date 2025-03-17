using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Membership
{
    public class CreateMembershipDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int MaxChildren { get; set; }
        public int MaxConsultations { get; set; }
        public bool CanAccessConsultation { get; set; }
    }
}
