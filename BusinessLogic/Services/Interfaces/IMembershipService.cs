using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.UserMembership;

namespace BusinessLogic.Services.Interfaces
{
    public interface IMembershipService
    {
        IEnumerable<MembershipDto> GetMembershipPlans();
        Task<bool> RegisterMembership(CreateUserMemebershipDTO userMembershipDto);
        Task<IEnumerable<UserMembershipDto>> ShowAllUserMemberships();
        Task<bool> UserMembershipStatus(int userMembershipId, bool newStatus, int userId);
    }
}
