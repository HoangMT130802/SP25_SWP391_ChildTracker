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
                // Kiểm tra child có thuộc về user không
                var childRepo = _unitOfWork.GetRepository<Child>();
                var child = await childRepo.GetAsync(c => c.ChildId == request.ChildId && c.UserId == userId);
                if (child == null)
                {
                    throw new InvalidOperationException("Không tìm thấy thông tin trẻ hoặc trẻ không thuộc về bạn");
                }

                var consultationRequest = _mapper.Map<ConsultationRequest>(request);
                consultationRequest.UserId = userId;

                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                await repo.AddAsync(consultationRequest);
                await _unitOfWork.SaveChangesAsync();

                // Tự động phân công bác sĩ
                await AutoAssignDoctorAsync(consultationRequest.RequestId);

                return await GetRequestByIdAsync(consultationRequest.RequestId, userId);
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

        public async Task<ConsultationResponseDTO> CreateResponseAsync(int requestId, int doctorId, CreateConsultationResponseDTO response)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Kiểm tra request có tồn tại và trạng thái hợp lệ
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(r => r.RequestId == requestId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Closed" || request.Status == "Expired")
                    throw new InvalidOperationException("Yêu cầu tư vấn đã đóng hoặc hết hạn");

                if (request.AssignedDoctorId != doctorId)
                    throw new InvalidOperationException("Bạn không được phân công cho yêu cầu tư vấn này");

                var consultationResponseRepo = _unitOfWork.GetRepository<ConsultationResponse>();

                // Kiểm tra câu hỏi tồn tại nếu có ParentResponseId
                if (response.ParentResponseId.HasValue)
                {
                    var question = await consultationResponseRepo.GetAsync(r =>
                        r.ResponseId == response.ParentResponseId.Value &&
                        r.RequestId == requestId &&
                        r.IsQuestion);

                    if (question == null)
                        throw new KeyNotFoundException("Không tìm thấy câu hỏi cần trả lời");
                }

                var consultationResponse = _mapper.Map<ConsultationResponse>(response);
                consultationResponse.RequestId = requestId;
                consultationResponse.DoctorId = doctorId;
                consultationResponse.IsFromUser = false;
                consultationResponse.IsQuestion = false;
                consultationResponse.CreatedAt = DateTime.UtcNow;
                consultationResponse.UpdatedAt = DateTime.UtcNow;

                await consultationResponseRepo.AddAsync(consultationResponse);

                // Cập nhật trạng thái và thời gian hoạt động của request
                request.Status = "InProgress";
                request.LastActivityAt = DateTime.UtcNow;
                requestRepo.Update(request);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ConsultationResponseDTO>(consultationResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo phản hồi tư vấn");
                throw;
            }
        }

        public async Task<ConsultationResponseDTO> AddUserQuestionAsync(int requestId, int userId, CreateConsultationResponseDTO questionDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(r => r.RequestId == requestId && r.UserId == userId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Completed" || request.Status == "Closed" || request.Status == "Expired")
                    throw new InvalidOperationException("Không thể thêm câu hỏi cho yêu cầu đã hoàn thành hoặc đã đóng");

                var response = _mapper.Map<ConsultationResponse>(questionDto);
                response.RequestId = requestId;
                response.CreatedAt = DateTime.UtcNow;
                response.UpdatedAt = DateTime.UtcNow;
                response.IsFromUser = true;
                response.IsQuestion = true;
                response.DoctorId = null;

                var responseRepo = _unitOfWork.GetRepository<ConsultationResponse>();
                await responseRepo.AddAsync(response);
                
                // Cập nhật trạng thái và thời gian hoạt động của request
                request.LastActivityAt = DateTime.UtcNow;
                if (request.Status == "Completed")
                {
                    request.Status = "InProgress"; // Mở lại request nếu người dùng có câu hỏi mới
                }
                requestRepo.Update(request);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ConsultationResponseDTO>(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi thêm câu hỏi cho yêu cầu tư vấn {RequestId}", requestId);
                throw;
            }
        }

        public async Task<ConsultationRequestDTO> GetRequestByIdAsync(int requestId, int userId)
        {
            try
            {
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(
                    r => r.RequestId == requestId && (r.UserId == userId || r.AssignedDoctorId == userId),
                    includeProperties: "User,AssignedDoctor,ConsultationResponses"
                );

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.UserId != userId && request.AssignedDoctorId != userId)
                    throw new UnauthorizedAccessException("Bạn không có quyền xem yêu cầu tư vấn này");

                return _mapper.Map<ConsultationRequestDTO>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin yêu cầu tư vấn {RequestId}", requestId);
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
                    includeProperties: "User,Child,AssignedDoctor,ConsultationResponses,ConsultationResponses.Doctor"
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
                    r => r.AssignedDoctorId == doctorId,
                    includeProperties: "User,Child,AssignedDoctor,ConsultationResponses,ConsultationResponses.Doctor"
                );

                return _mapper.Map<IEnumerable<ConsultationRequestDTO>>(requests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy danh sách yêu cầu tư vấn của bác sĩ {doctorId}");
                throw;
            }
        }

        public async Task<ConsultationRequestDTO> MarkRequestAsSatisfiedAsync(int requestId, int userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await repo.GetAsync(r => r.RequestId == requestId && r.UserId == userId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Closed" || request.Status == "Expired")
                    throw new InvalidOperationException("Yêu cầu tư vấn đã đóng hoặc hết hạn");

                request.Status = "Closed";
                request.IsSatisfied = true;
                request.ClosedReason = "Người dùng hài lòng với câu trả lời";
                request.ClosedAt = DateTime.UtcNow;

                repo.Update(request);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetRequestByIdAsync(requestId, userId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi khi đánh dấu hài lòng cho yêu cầu {requestId}");
                throw;
            }
        }

        public async Task<ConsultationRequestDTO> CloseRequestAsync(int requestId, string reason, string closedBy)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var repo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await repo.GetAsync(r => r.RequestId == requestId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Closed" || request.Status == "Expired")
                    throw new InvalidOperationException("Yêu cầu tư vấn đã đóng hoặc hết hạn");

                request.Status = "Closed";
                request.ClosedReason = $"Đóng bởi {closedBy}. Lý do: {reason}";
                request.ClosedAt = DateTime.UtcNow;

                repo.Update(request);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetRequestByIdAsync(requestId, request.UserId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi khi đóng yêu cầu tư vấn {requestId}");
                throw;
            }
        }

        public async Task<ConsultationResponseDTO> UpdateResponseAsync(int responseId, string newResponse)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var responseRepo = _unitOfWork.GetRepository<ConsultationResponse>();
                var response = await responseRepo.GetAsync(
                    r => r.ResponseId == responseId,
                    includeProperties: "Request,Doctor"
                );

                if (response == null)
                    throw new KeyNotFoundException($"Không tìm thấy phản hồi ID {responseId}");

                if (response.Request.Status == "Closed" || response.Request.Status == "Expired")
                    throw new InvalidOperationException("Không thể cập nhật phản hồi cho yêu cầu đã đóng hoặc hết hạn");

                response.Response = newResponse;
                response.UpdatedAt = DateTime.UtcNow;

                // Cập nhật thời gian hoạt động của request
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(r => r.RequestId == response.RequestId);
                request.LastActivityAt = DateTime.UtcNow;

                responseRepo.Update(response);
                requestRepo.Update(request);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ConsultationResponseDTO>(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi khi cập nhật phản hồi {responseId}");
                throw;
            }
        }

        public async Task<ConsultationRequestDTO> AssignDoctorAsync(int requestId, int doctorId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
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
                request.AssignedDoctorId = doctorId;
                request.LastActivityAt = DateTime.UtcNow;

                requestRepo.Update(request);

                // Tạo response thông báo phân công
                var responseRepo = _unitOfWork.GetRepository<ConsultationResponse>();
                await responseRepo.AddAsync(new ConsultationResponse
                {
                    RequestId = requestId,
                    DoctorId = doctorId,
                    Response = "Bác sĩ đã được phân công xử lý yêu cầu này",
                    IsFromUser = false,
                    IsQuestion = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Attachments = ""
                });

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetRequestByIdAsync(requestId, request.UserId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi khi phân công bác sĩ cho yêu cầu {requestId}");
                throw;
            }
        }

        public async Task<Dictionary<int, int>> GetDoctorWorkloadAsync()
        {
            try
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
                        r.AssignedDoctorId == doctor.UserId &&
                        r.Status != "Closed" &&
                        r.Status != "Expired");

                    workload.Add(doctor.UserId, activeRequests.Count());
                }

                return workload;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin khối lượng công việc của bác sĩ");
                throw;
            }
        }

        public async Task<ConsultationRequestDTO> CompleteRequestAsync(int requestId, int userId, bool isSatisfied)
        {
            try
            {
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(r => r.RequestId == requestId && r.UserId == userId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Completed")
                    throw new InvalidOperationException("Yêu cầu tư vấn đã được hoàn thành");

                request.Status = "Completed";
                request.IsSatisfied = isSatisfied;
                request.LastActivityAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ConsultationRequestDTO>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hoàn thành yêu cầu tư vấn {RequestId}", requestId);
                throw;
            }
        }

        public async Task CheckAndUpdateExpiredRequestsAsync()
        {
            try
            {
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var expirationTime = DateTime.UtcNow.AddHours(-24);
                var expiredRequests = await requestRepo.FindAsync(
                    r => r.Status == "Pending" && r.LastActivityAt < expirationTime
                );

                foreach (var request in expiredRequests)
                {
                    request.Status = "Expired";
                    request.LastActivityAt = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái các yêu cầu tư vấn hết hạn");
                throw;
            }
        }

        public async Task<ConsultationResponseDTO> AddQuestionAsync(int requestId, int userId, AskQuestionDTO questionDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(r => r.RequestId == requestId && r.UserId == userId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Completed" || request.Status == "Closed" || request.Status == "Expired")
                    throw new InvalidOperationException("Không thể thêm câu hỏi cho yêu cầu đã hoàn thành hoặc đã đóng");

                var consultationResponseRepo = _unitOfWork.GetRepository<ConsultationResponse>();

                // Kiểm tra câu trả lời tồn tại nếu đang hỏi thêm
                if (questionDto.ReplyToResponseId.HasValue)
                {
                    var parentResponse = await consultationResponseRepo.GetAsync(r => 
                        r.ResponseId == questionDto.ReplyToResponseId.Value && 
                        r.RequestId == requestId);

                    if (parentResponse == null)
                        throw new KeyNotFoundException("Không tìm thấy câu trả lời để hỏi thêm");
                }

                var response = _mapper.Map<ConsultationResponse>(questionDto);
                response.RequestId = requestId;
                response.DoctorId = null;

                await consultationResponseRepo.AddAsync(response);
                
                // Cập nhật trạng thái và thời gian hoạt động của request
                request.LastActivityAt = DateTime.UtcNow;
                if (request.Status == "Completed")
                {
                    request.Status = "InProgress"; // Mở lại request nếu người dùng có câu hỏi mới
                }
                requestRepo.Update(request);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ConsultationResponseDTO>(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi thêm câu hỏi cho yêu cầu tư vấn {RequestId}", requestId);
                throw;
            }
        }

        public async Task<ConsultationResponseDTO> AddDoctorResponseAsync(int requestId, int doctorId, DoctorResponseDTO responseDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var requestRepo = _unitOfWork.GetRepository<ConsultationRequest>();
                var request = await requestRepo.GetAsync(r => r.RequestId == requestId);

                if (request == null)
                    throw new KeyNotFoundException("Không tìm thấy yêu cầu tư vấn");

                if (request.Status == "Closed" || request.Status == "Expired")
                    throw new InvalidOperationException("Yêu cầu tư vấn đã đóng hoặc hết hạn");

                if (request.AssignedDoctorId != doctorId)
                    throw new InvalidOperationException("Bạn không được phân công cho yêu cầu tư vấn này");

                // Kiểm tra câu hỏi tồn tại
                var responseRepo = _unitOfWork.GetRepository<ConsultationResponse>();
                var question = await responseRepo.GetAsync(r =>
                    r.ResponseId == responseDto.QuestionId &&
                    r.RequestId == requestId &&
                    r.IsQuestion);

                if (question == null)
                    throw new KeyNotFoundException("Không tìm thấy câu hỏi cần trả lời");

                var response = _mapper.Map<ConsultationResponse>(responseDto);
                response.RequestId = requestId;
                response.DoctorId = doctorId;

                await responseRepo.AddAsync(response);

                // Cập nhật trạng thái và thời gian hoạt động của request
                request.Status = "InProgress";
                request.LastActivityAt = DateTime.UtcNow;
                requestRepo.Update(request);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<ConsultationResponseDTO>(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo phản hồi tư vấn");
                throw;
            }
        }
    }
}