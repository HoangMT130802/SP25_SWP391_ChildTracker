
using Azure;
using BusinessLogic.DTOs.Children;
using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;



namespace BusinessLogic.Services.Implementations
{
    public class ChildrenService : IChildrenService
    {
        private readonly IGenericRepository<Child> _childrenRepository;
        private readonly IGenericRepository<User> _userRepository;
        public ChildrenService(IGenericRepository<Child> childrenRepository, IGenericRepository<User> userRepository)
        {
            _childrenRepository = childrenRepository;
            _userRepository = userRepository;
        }


        // ListChildren Không phân trang 
        public async Task<List<ChildrenDTO>> GetChildrenByUserIdAsync(int userId)
        {
            var allChildren = await _childrenRepository.GetAllQueryable()
                .Where(ch => ch.UserId == userId)
                .Select(ch => new ChildrenDTO
                {
                    child_id = ch.ChildId,
                    FullName = ch.FullName,
                    ParentName = ch.ParentName,
                    ParentNumber = ch.ParentNumber,
                    birth_date = ch.BirthDate,
                    gender = ch.Gender,
                    BloodType = ch.BloodType,
                    AllergiesNotes = ch.AllergiesNotes,
                    MedicalHistory = ch.MedicalHistory,
                    Status = ch.Status,
                    CreatedAt = ch.CreatedAt,
                    UpdatedAt = ch.UpdateAt
                }).ToListAsync();
            return allChildren;
        }

        // ListChildren Có phân trang
        public async Task<PaginatedList<ChildrenDTO>> GetChildrenPaginationAsync(int userId, int page = 1, int page_size = 5)
        {
            var allChildren = await GetChildrenByUserIdAsync(userId);
            // Chuyển danh sách thành IQueryable để phân trang
            var query = allChildren.AsQueryable();
            // Sử dụng PaginatedList<T> để phân trang
            var pagedResult = PaginatedList<ChildrenDTO>.Create(query, page, page_size);
            return pagedResult;
        }


        // tìm kiếm theo ID
        public async Task<ChildrenDTO> GetChildByIdAsync(int userId , int childId)
        {
            var childid = await _childrenRepository.GetAllQueryable()
                .Where(ch => ch.ChildId == childId && ch.UserId == userId)
                .FirstOrDefaultAsync();
            if (childid == null)
            {
                throw new Exception("Không có thông tin trẻ");
            }

            return new ChildrenDTO
            {
                child_id = childid.ChildId,
                FullName = childid.FullName,
                ParentName = childid.ParentName,
                ParentNumber = childid.ParentNumber,
                birth_date = childid.BirthDate,
                gender = childid.Gender,
                BloodType = childid.BloodType,
                AllergiesNotes = childid.AllergiesNotes,
                MedicalHistory = childid.MedicalHistory,
                Status = childid.Status,
                CreatedAt = childid.CreatedAt,
                UpdatedAt = childid.UpdateAt
            };
        }

        // Tìm Trẻ theo tên
        public async Task<List<ChildrenDTO>> SearchNameChild(String search, int userId)
        {
            var result = await _childrenRepository.GetAllQueryable()
                .Where(ch => ch.FullName.ToLower().Contains(search.ToLower()) && ch.UserId == userId)
                .ToListAsync();
            if (result == null)
            {
                throw new Exception("Tên trẻ không tồn tại");
            }

            return result.Select(ch => new ChildrenDTO
            {
                child_id = ch.ChildId,
                FullName = ch.FullName,
                ParentName = ch.ParentName,
                ParentNumber = ch.ParentNumber,
                birth_date = ch.BirthDate,
                gender = ch.Gender,
                BloodType = ch.BloodType,
                AllergiesNotes = ch.AllergiesNotes,
                MedicalHistory = ch.MedicalHistory,
                Status = ch.Status,
                CreatedAt = ch.CreatedAt,
                UpdatedAt = ch.UpdateAt
            }).ToList(); 
        }


        public async Task<ChildrenDTO> CreateChildAsync(CreateChildrenDTO createDTO, int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Người dùng không tồn tại");
            }
            if (user.Role != "User")
            {
                throw new Exception("Không thể tạo thông tin trẻ");
            }

            var child = new Child
            {
                UserId = userId,
                FullName = createDTO.FullName,
                ParentName = createDTO.ParentName,
                ParentNumber = createDTO.ParentNumber,
                BirthDate = createDTO.birth_date,
                Gender = createDTO.gender,
                BloodType = createDTO.BloodType,
                AllergiesNotes = createDTO.AllergiesNotes,
                MedicalHistory = createDTO.MedicalHistory,
                Status = true,
                CreatedAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            await _childrenRepository.AddAsync(child);
            await _childrenRepository.SaveAsync();

            return new ChildrenDTO
            {
                child_id = child.ChildId,
                FullName = child.FullName,
                ParentName = child.ParentName,
                ParentNumber = child.ParentNumber,
                BloodType = child.BloodType,
                AllergiesNotes = child.AllergiesNotes,
                MedicalHistory = child.MedicalHistory,
                Status = child.Status,
                CreatedAt = child.CreatedAt,
                UpdatedAt = child.UpdateAt
            };
        }


        // cập nhật trẻ
        public async Task<ChildrenDTO> UpdateChildAsync(int userId, int childId, UpdateChildrenDTO updateDTO)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            var child = await _childrenRepository.GetByIdAsync(childId);

            if (user == null) { throw new Exception("Người dùng không tồn tại"); }

            if (user.Role != "User" && child.UserId != userId) { throw new Exception("Không thể sửa thông tin trẻ"); }

            if (child == null) { throw new Exception("Trẻ không tồn tại"); }

            child.FullName = updateDTO.FullName;
            child.ParentName = updateDTO.ParentName;
            child.ParentNumber = updateDTO.ParentNumber;
            child.BirthDate = updateDTO.birth_date;
            child.Gender = updateDTO.gender;
            child.BloodType = updateDTO.BloodType;
            child.AllergiesNotes = updateDTO.AllergiesNotes;
            child.MedicalHistory = updateDTO.MedicalHistory;
            child.UpdateAt = DateTime.UtcNow;

            _childrenRepository.Update(child);
            await _childrenRepository.SaveAsync();

            return new ChildrenDTO
            {
                child_id = child.ChildId,
                FullName = child.FullName,
                ParentName = child.ParentName,
                ParentNumber = child.ParentNumber,
                BloodType = child.BloodType,
                AllergiesNotes = child.AllergiesNotes,
                MedicalHistory = child.MedicalHistory,
                Status = child.Status,
                UpdatedAt = child.UpdateAt
            };

        }


        // xóa trẻ
        public async Task DeleteChildAsync(int childId)
        {
            var child = await _childrenRepository.GetByIdAsync(childId);
            if (child == null) { throw new Exception("Trẻ không tồn tại"); }

            _childrenRepository.Delete(child);
            await _childrenRepository.SaveAsync();
        }
    }
}
