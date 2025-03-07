using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories;
using global::BusinessLogic.DTOs.UserMembership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
        public class MembershipService : IMembershipService
    {
        private readonly IGenericRepository<UserMembership> _userMembershipRepository;
        private readonly IGenericRepository<User> _userRepository;

        public MembershipService(IGenericRepository<UserMembership> userMembershipRepository, 
            IGenericRepository<User> userRepository, List<Membership> fixedMemberships)
        {
            _userMembershipRepository = userMembershipRepository;
            _userRepository = userRepository;
            _fixedMemberships = fixedMemberships;
        }


        //  2 gói membership cứng
        private readonly List<Membership> _fixedMemberships = new List<Membership>
        {
            new Membership
            {
                MembershipId = 1,
                Name = "Gói Standard",
                Description = "Basic membership with limited features",
                Duration = 30,
                Price = 100.000m,
                MaxChildren = 15,
                MaxConsultations = 0,
                CanAccessConsultation = false,
                Status = true
            },
            new Membership
            {
                MembershipId = 2,
                Name = "Gói VIP",
                Description = "Premium membership with full features",
                Duration = 30,
                Price = 500.000m,
                MaxChildren = 30,
                MaxConsultations = 30,
                CanAccessConsultation = true,
                Status = true
            }
        };

            // Hiển thị danh sách Membership
            public IEnumerable<MembershipDto> GetMembershipPlans()
            {
                return _fixedMemberships.Select(m => new MembershipDto
                {
                    MembershipId = m.MembershipId,
                    Name = m.Name,
                    Description = m.Description,
                    Duration = m.Duration,
                    Price = $"{m.Price} VNĐ",
                    MaxChildren = m.MaxChildren,
                    MaxConsultations = m.MaxConsultations
                });
            }

            // Đăng ký membership chưa qua thành toán
            public async Task<bool> RegisterMembership(CreateUserMemebershipDTO userMembershipDto)
            {
                var membership = _fixedMemberships.FirstOrDefault(m => m.MembershipId == userMembershipDto.MembershipId);
                if (membership == null)
                    return false;

                var newUserMembership = new UserMembership
                {
                    UserId = userMembershipDto.UserId,
                    MembershipId = membership.MembershipId,
                    Status = "False", // Chưa thanh toán
                    RemainingConsultations = membership.MaxConsultations
                };

                await _userMembershipRepository.AddAsync(newUserMembership);
                
                return true;
            }

        // lấy tất cả usermembership
        public async Task<IEnumerable<UserMembershipDto>> ShowAllUserMemberships()
        {
            var userMemberships = await _userMembershipRepository.GetAllAsync();

            return userMemberships.Select(u => new UserMembershipDto
            {
                UserMembershipId = u.UserMembershipId,
                UserId = u.UserId,
                MembershipId = u.MembershipId,
                StartDate = u.StartDate,
                EndDate = u.EndDate,
                Status = u.Status,
                RemainingConsultations = u.RemainingConsultations,
                LastRenewalDate = u.LastRenewalDate
            }).ToList();
        }


        // Quản lý trạng thái gói của Usermembership
        public async Task<bool> UserMembershipStatus(int userMembershipId, bool newStatus, int userId)
        {
            // Kiểm tra user có quyền admin không
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Role != "Admin")
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này.");
            }

            // Tìm UserMembership
            var userMembership = await _userMembershipRepository.GetByIdAsync(userMembershipId);
            if (userMembership == null)
            {
                throw new Exception("Không tìm thấy UserMembership.");
            }

            // Cập nhật trạng thái
            userMembership.Status = newStatus ? "True" : "False";
            _userMembershipRepository.Update(userMembership);

            return true;
        }

        // nâng cấp gói
        public async Task<bool> UpgradeMembership(int userMembershipId)
        {
            try
            {
                var userMembership = await _userMembershipRepository.GetByIdAsync(userMembershipId);
                if (userMembership == null)
                {
                    throw new Exception("Không tìm thấy UserMembership.");
                }
                // Kiểm tra có đang ở gói Standard không
                if (userMembership.MembershipId != 1)
                {
                    throw new Exception("Chỉ có thể nâng cấp từ Gói Standard lên Gói VIP.");
                }
                // Lấy thông tin gói VIP
                var vipMembership = _fixedMemberships.FirstOrDefault(m => m.MembershipId == 2);
                if (vipMembership == null)
                {
                    throw new Exception("Không tìm thấy gói VIP.");
                }
                // TODO: hàm thanh
                userMembership.MembershipId = vipMembership.MembershipId;
                userMembership.RemainingConsultations = vipMembership.MaxConsultations;

                // Cập nhật thời gian
                userMembership.StartDate = DateTime.UtcNow;
                userMembership.EndDate = userMembership.StartDate.AddDays(vipMembership.Duration);

                // Lưu thay đổi
                _userMembershipRepository.Update(userMembership);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi nâng cấp membership: {ex.Message}");
                return false;
            }
        }
    }
}

