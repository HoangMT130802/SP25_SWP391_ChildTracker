
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
        }


        public async Task<ChildDTO> UpdateChildAsync(int childId, int userId, UpdateChildDTO childDTO)
        {
            try
            {
                var childRepository = _unitOfWork.GetRepository<Child>();
                var child = await childRepository.GetAsync(c => c.ChildId == childId && c.UserId == userId);

                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {childId} not found for user {userId}");
                }

                _mapper.Map(childDTO, child);
                child.UpdateAt = DateTime.UtcNow;

                childRepository.Update(child);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ChildDTO>(child);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating child {childId} for user {userId}");
                throw;
            }
        }

        public async Task<bool> SoftDeleteChildAsync(int childId, int userId)
        {
            try
            {
                var childRepository = _unitOfWork.GetRepository<Child>();
                var child = await childRepository.GetAsync(c => c.ChildId == childId && c.UserId == userId);

                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {childId} not found for user {userId}");
                }

                // Soft delete
                child.Status = false;
                child.UpdateAt = DateTime.UtcNow;

                childRepository.Update(child);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting child {childId} for user {userId}");
                throw;
            }
        }
        public async Task<bool> HardDeleteChildAsync(int childId, int userId)
        {
            try
            {
                var childRepository = _unitOfWork.GetRepository<Child>();

                var child = await childRepository.GetAsync(c => c.ChildId == childId && c.UserId == userId);

                if (child == null)
                {
                    throw new KeyNotFoundException($"Child with ID {childId} not found for user {userId}");
                }

                childRepository.Delete(child);

                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error hard deleting child {childId} for user {userId}");
                throw;
            }
        }
    }
}
