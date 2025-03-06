using AutoMapper;
using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.Services.Interfaces;
using DataAccess.Entities;
using DataAccess.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Implementations
{
    public class ConsultationService : IConsultationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsultationService> _logger;
        private const int REQUEST_EXPIRY_HOURS = 24;

        public ConsultationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ConsultationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ConsultationRequestDTO> CreateRequestAsync(int userId, CreateConsultationRequestDTO request)
        {
            try
            {
                var consultationRequest = new ConsultationRequest
                {
                    UserId = userId,
                    ChildId = request.ChildId,
                    Description = request.Description,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                await repo.AddAsync(consultationRequest);
                await _unitOfWork.SaveChangesAsync();

                // Tự động phân công bác sĩ
                await AutoAssignDoctorAsync(consultationRequest.RequestId);

                return _mapper.Map<ConsultationRequestDTO>(consultationRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo yêu cầu tư vấn");
                throw;
            }
        }


        private async Task AutoAssignDoctorAsync(int requestId)
        {
            var doctorWorkload = await GetDoctorWorkloadAsync();
            if (!doctorWorkload.Any()) return;

            // Chọn bác sĩ có ít việc nhất
            var doctorId = doctorWorkload.OrderBy(x => x.Value).First().Key;
            await AssignDoctorAsync(requestId, doctorId);
        }

        public async Task<ConsultationResponseDTO> CreateResponseAsync(int doctorId, CreateConsultationResponseDTO response)
        {
            try
            {
                // Kiểm tra request có tồn tại và trạng thái hợp lệ
                var request = await _unitOfWork.GetRepository<ConsultationRequest>()
                    .GetAsync(r => r.RequestId == response.RequestId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Closed" || request.Status == "Expired")
                    throw new InvalidOperationException("Yêu cầu tư vấn đã đóng hoặc hết hạn");

                var consultationResponse = new ConsultationResponse
                {
                    RequestId = response.RequestId,
                    DoctorId = doctorId,
                    Response = response.Response,
/*                    Attachments = response.Attachments,*/
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var repo = _unitOfWork.GetRepository<ConsultationResponse>();
                await repo.AddAsync(consultationResponse);

                // Cập nhật trạng thái request
                request.Status = "InProgress";
                _unitOfWork.GetRepository<ConsultationRequest>().Update(request);

                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ConsultationResponseDTO>(consultationResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo phản hồi tư vấn");
                throw;
            }
        }

        public async Task<ConsultationRequestDTO> CloseRequestAsync(int requestId, string reason, string closedBy)
        {
            try
            {
                var request = await _unitOfWork.GetRepository<ConsultationRequest>()
                    .GetAsync(r => r.RequestId == requestId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                request.Status = "Closed";
                _unitOfWork.GetRepository<ConsultationRequest>().Update(request);

                // Thêm response ghi nhận việc đóng
                var closeResponse = new ConsultationResponse
                {
                    RequestId = requestId,
                    DoctorId = 0, // System
                    Response = $"Yêu cầu đã được đóng bởi {closedBy}. Lý do: {reason}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GetRepository<ConsultationResponse>().AddAsync(closeResponse);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ConsultationRequestDTO>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đóng yêu cầu tư vấn");
                throw;
            }
        }

        public async Task<bool> CheckAndUpdateExpiredRequestsAsync()
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                var expiryDate = DateTime.UtcNow.AddHours(-REQUEST_EXPIRY_HOURS);

                // Lấy các request chưa đóng và không có hoạt động trong 24h
                var expiredRequests = await repo.FindAsync(r =>
                    r.Status != "Closed" &&
                    r.Status != "Expired" &&
                    r.CreatedAt <= expiryDate &&
                    !r.ConsultationResponses.Any(cr => cr.CreatedAt > expiryDate));

                foreach (var request in expiredRequests)
                {
                    request.Status = "Expired";
                    repo.Update(request);
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra và cập nhật yêu cầu hết hạn");
                return false;
            }
        }

        public async Task<Dictionary<int, int>> GetDoctorWorkloadAsync()
        {
            var doctorRepo = _unitOfWork.GetRepository<User>();
            var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();

            // Lấy danh sách bác sĩ
            var doctors = await doctorRepo.FindAsync(u => u.Role == "Doctor");
            var workload = new Dictionary<int, int>();

            foreach (var doctor in doctors)
            {
                // Đếm số lượng request đang xử lý của mỗi bác sĩ
                var activeRequests = await requestRepo.FindAsync(r =>
                    r.ConsultationResponses.Any(cr => cr.DoctorId == doctor.UserId) &&
                    r.Status != "Closed" &&
                    r.Status != "Expired");

                workload.Add(doctor.UserId, activeRequests.Count());
            }

            return workload;
        }
        public async Task<ConsultationRequestDTO> GetRequestByIdAsync(int requestId)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await repo.GetAsync(
                    r => r.RequestId == requestId,
                    includeProperties: "User,Child,ConsultationResponses,ConsultationResponses.Doctor"
                );

                if (request == null)
                    throw new KeyNotFoundException($"Không tìm thấy yêu cầu tư vấn ID {requestId}");

                return _mapper.Map<ConsultationRequestDTO>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy yêu cầu tư vấn {requestId}");
                throw;
            }
        }

        public async Task<IEnumerable<ConsultationRequestDTO>> GetUserRequestsAsync(int userId)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                var requests = await repo.FindAsync(
                    r => r.UserId == userId,
                    includeProperties: "User,Child,ConsultationResponses,ConsultationResponses.Doctor"
                );

                return _mapper.Map<IEnumerable<ConsultationRequestDTO>>(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách yêu cầu tư vấn của user {userId}");
                throw;
            }
        }

        public async Task<IEnumerable<ConsultationRequestDTO>> GetDoctorRequestsAsync(int doctorId)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                var requests = await repo.FindAsync(
                    r => r.ConsultationResponses.Any(cr => cr.DoctorId == doctorId),
                    includeProperties: "User,Child,ConsultationResponses,ConsultationResponses.Doctor"
                );

                return _mapper.Map<IEnumerable<ConsultationRequestDTO>>(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách yêu cầu tư vấn của bác sĩ {doctorId}");
                throw;
            }
        }

        public async Task<ConsultationResponseDTO> UpdateResponseAsync(int responseId, UpdateResponseDTO updateRequest)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ConsultationResponse>();
                var response = await repo.GetAsync(r => r.ResponseId == responseId);

                if (response == null)
                    throw new KeyNotFoundException($"Không tìm thấy phản hồi ID {responseId}");

                response.Response = updateRequest.NewResponse;
                response.Attachments = updateRequest.Attachments;
                response.UpdatedAt = DateTime.UtcNow;

                repo.Update(response);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ConsultationResponseDTO>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi cập nhật phản hồi {responseId}");
                throw;
            }
        }

        public async Task<ConsultationRequestDTO> AssignDoctorAsync(int requestId, int doctorId)
        {
            try
            {
                // Kiểm tra bác sĩ có tồn tại
                var doctorRepo = _unitOfWork.GetRepository<User>();
                var doctor = await doctorRepo.GetAsync(d => d.UserId == doctorId && d.Role == "Doctor");

                if (doctor == null)
                    throw new InvalidOperationException($"Không tìm thấy bác sĩ ID {doctorId}");

                // Kiểm tra và cập nhật request
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(r => r.RequestId == requestId);

                if (request == null)
                    throw new KeyNotFoundException($"Không tìm thấy yêu cầu tư vấn ID {requestId}");

                if (request.Status != "Pending")
                    throw new InvalidOperationException("Yêu cầu tư vấn không ở trạng thái chờ xử lý");

                request.Status = "Assigned";
                requestRepo.Update(request);

                // Tạo response thông báo phân công
                var responseRepo = _unitOfWork.GetRepository<ConsultationResponse>();
                await responseRepo.AddAsync(new ConsultationResponse
                {
                    RequestId = requestId,
                    DoctorId = doctorId,
                    Response = "Bác sĩ đã được phân công xử lý yêu cầu này",
                    //
                    Attachments = "",
                    //
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();

                return await GetRequestByIdAsync(requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi phân công bác sĩ cho yêu cầu {requestId}");
                throw;
            }
        }
    }
}