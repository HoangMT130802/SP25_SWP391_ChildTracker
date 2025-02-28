using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IAdminService
    {
        Task<Dictionary<string, int>> GetUserStatisticsRoleAsync();

        Task<object> GetUsersCreatedOnDateAsync(string? date);
    }
}
