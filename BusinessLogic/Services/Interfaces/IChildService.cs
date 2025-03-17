using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Children;
using DataAccess.Entities;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Interfaces
{
    public interface IChildService
    {
        Task<IEnumerable<ChildDTO>> GetAllChildrenByUserIdAsync(int userId);
        Task<ChildDTO> GetChildByIdAsync(int childId, int userId);
        Task<ChildDTO> CreateChildAsync(int userId, CreateChildDTO childDTO);
        Task<List<ChildrenDTO>> SearchNameChild(String search, int userId);
        Task<ChildrenDTO> UpdateChildAsync(int userId, int childId, UpdateChildrenDTO updateDTO);
        Task DeleteChildAsync(int childId);
    }
}
