
using AutoMapper;
using Azure;
using BusinessLogic.DTOs.Children;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DataAccess.UnitOfWork;


namespace BusinessLogic.Services.Implementations
{
    public class ChildService : IChildService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ChildService> _logger;

        public ChildService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ChildService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<IEnumerable<ChildDTO>> GetAllChildrenByUserIdAsync(int userId)
        {
            try
            {
                var childRepository = _unitOfWork.GetRepository<Child>();
                var children = await childRepository.FindAsync(c => c.UserId == userId);
                return _mapper.Map<IEnumerable<ChildDTO>>(children);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting children for user {userId}");
                throw;
            }
        }

        public async Task<ChildDTO> GetChildByIdAsync(int childId, int userId)
        {
            try
            {
                var childRepository = _unitOfWork.GetRepository<Child>();
                var child = await childRepository.GetAsync(c => c.ChildId == childId && c.UserId == userId);

                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {childId} not found for user {userId}");
                }

                return _mapper.Map<ChildDTO>(child);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting child {childId} for user {userId}");
                throw;
            }
        }

        public async Task<ChildDTO> CreateChildAsync(int userId, CreateChildDTO childDTO)
        {
            try
            {
                var childRepository = _unitOfWork.GetRepository<Child>();

                var child = _mapper.Map<Child>(childDTO);
                child.UserId = userId;
                child.Status = true;
                child.CreatedAt = DateTime.UtcNow;
                child.UpdateAt = DateTime.UtcNow;

                await childRepository.AddAsync(child);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ChildDTO>(child);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating child for user {userId}");
                throw;
            }
        }//


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
