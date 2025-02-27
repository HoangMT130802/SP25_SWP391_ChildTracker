using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.DTOs.Children;
using DataAccess.Models;
using DataAccess.Repositories;

namespace BusinessLogic.Services.Interfaces
{
    public interface IChildrenService
    {
        Task<List<ChildrenDTO>> GetChildrenByUserIdAsync(int userId);
        Task<PaginatedList<ChildrenDTO>> GetChildrenPaginationAsync(int userId, int page, int page_size);
        Task<ChildrenDTO> GetChildByIdAsync(int userId, int childId);
        Task<List<ChildrenDTO>> SearchNameChild(String search, int userId);
        Task<ChildrenDTO> CreateChildAsync(CreateChildrenDTO createDTO, int userId);
        Task<ChildrenDTO> UpdateChildAsync(int userId, int childId, UpdateChildrenDTO updateDTO);
        Task DeleteChildAsync(int childId);
    }
}
