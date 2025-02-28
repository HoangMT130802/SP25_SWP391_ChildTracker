using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<User> _userRepository;

        public AdminService(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        // Thống kê các role
        public async Task<Dictionary<string, int>> GetUserStatisticsRoleAsync()
        {
            var users = await _userRepository.GetAllAsync() ?? new List<User>(); // Xử lý null

            Dictionary<string, int> roleCounts = new Dictionary<string, int>();

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.Role))
                    continue; // Bỏ qua user không có Role

                if (roleCounts.TryGetValue(user.Role, out int count))
                {
                    roleCounts[user.Role] = count + 1;
                }
                else
                {
                    roleCounts[user.Role] = 1;
                }
            }

            return roleCounts;
        }


        public async Task<object> GetUsersCreatedOnDateAsync(string? date)
        {
            DateTime selectedDate;

            // Kiểm tra và chuyển đổi ngày
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out selectedDate))
            {
                selectedDate = selectedDate.Date;
            }
            else
            {
                selectedDate = DateTime.UtcNow.Date;
            }

            var users = await _userRepository.FindAsync(u => u.CreatedAt.Date == selectedDate);

            return new
            {
                date = selectedDate.ToString("yyyy-MM-dd"),
                totalCreatedUsers = users.Count(),
                users = users
            };
        }
    }
}
