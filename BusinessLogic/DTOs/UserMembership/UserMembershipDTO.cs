using BusinessLogic.DTOs.Membership;
using BusinessLogic.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.UserMembership
{
    public class UserMembershipDTO
    {
        public int UserMembershipId { get; set; }
        public int UserId { get; set; }
        public int MembershipId { get; set; }
        public string MembershipName { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
