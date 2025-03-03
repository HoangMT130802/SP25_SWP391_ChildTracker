using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IConsultationService
    {
        // Yêu cầu tư vấn
        Task<ConsultationRequestDTO> CreateRequestAsync(int userId, CreateConsultationRequestDTO request);
        Task<ConsultationRequestDTO> GetRequestByIdAsync(int requestId);
        Task<IEnumerable<ConsultationRequestDTO>> GetUserRequestsAsync(int userId);
        Task<IEnumerable<ConsultationRequestDTO>> GetDoctorRequestsAsync(int doctorId);

        // Phản hồi tư vấn
        Task<ConsultationResponseDTO> CreateResponseAsync(int doctorId, CreateConsultationResponseDTO response);
        Task<ConsultationResponseDTO> UpdateResponseAsync(int responseId, string newResponse);

        // Quản lý trạng thái
        Task<ConsultationRequestDTO> AssignDoctorAsync(int requestId, int doctorId);
        Task<ConsultationRequestDTO> CloseRequestAsync(int requestId, string reason, string closedBy);
        Task<bool> CheckAndUpdateExpiredRequestsAsync();

        // Thống kê
        Task<Dictionary<int, int>> GetDoctorWorkloadAsync();
    }
}
