using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.UserMembership
{
    public class UserMembershipDto
    {
        public int UserMembershipId { get; set; }
        public int UserId { get; set; }
        public int MembershipId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int RemainingConsultations { get; set; }
        public DateTime? LastRenewalDate { get; set; }
    }
}
