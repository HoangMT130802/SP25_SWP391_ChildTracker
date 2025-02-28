
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


        public async Task<object> TotalUsersCreateByDateAsync(string? date)
        {
            try
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
                    totalCreatedUsers = users.Count()
                };
            }
            catch (FormatException)
            {
                return new { error = "Định dạng không hợp lệ. Vui lòng sử dụng 'yyyy-MM-dd'." };
            }
        }

        public async Task<object> TotalUsersCreateByMonthAsync(string? monthYear)
        {
            try
            {
                DateTime selectedMonth;
                // Nếu không có input, lấy tháng hiện tại
                if (string.IsNullOrEmpty(monthYear))
                {
                    selectedMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                }
                else
                {
                    selectedMonth = DateTime.ParseExact(monthYear, "yyyy-MM", null);
                }

                // Lấy danh sách user đăng ký trong tháng
                var users = await _userRepository.FindAsync(u =>
                    u.CreatedAt.Year == selectedMonth.Year &&
                    u.CreatedAt.Month == selectedMonth.Month);

                return new
                {
                    month = selectedMonth.ToString("yyyy-MM"),
                    totalCreatedUsers = users.Count()
                };
            }
            catch (FormatException)
            {
                return new { error = "Định dạng không hợp lệ. Vui lòng sử dụng 'yyyy-MM'." };
            }
        }

        public async Task<object> GetUserStatusStatisticsAsync()
        {
            try
            {
                var allUsers = await _userRepository.GetAllAsync();

                var activeCount = allUsers.Count(u => u.Status == true);
                var blockedCount = allUsers.Count(u => u.Status == false);

                return new
                {
                    totalUsers = allUsers.Count(),
                    statusBreakdown = new[]
                    {
                new { Status = "Users Active", Total  = activeCount },
                new { Status = "Users Blocked", Total  = blockedCount }
            }
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    error = "Lỗi xử lý dữ liệu",
                    details = ex.Message
                };
            }
        }

    }
}
