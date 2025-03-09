using BusinessLogic.DTOs.ConsultationRequest;
using BusinessLogic.DTOs.ConsultationResponse;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Interfaces
{
    public interface IConsultationService
    {
        // Request methods
        Task<ConsultationRequestDTO> CreateRequestAsync(int userId, CreateConsultationRequestDTO request);
        Task<ConsultationRequestDTO> GetRequestByIdAsync(int requestId, int userId);
        Task<IEnumerable<ConsultationRequestDTO>> GetUserRequestsAsync(int userId);
        Task<IEnumerable<ConsultationRequestDTO>> GetDoctorRequestsAsync(int doctorId);
        Task<ConsultationRequestDTO> CompleteRequestAsync(int requestId, int userId, bool isSatisfied);

        // Response methods
        Task<ConsultationResponseDTO> CreateResponseAsync(int doctorId, CreateConsultationResponseDTO response);
        Task<ConsultationResponseDTO> AddUserQuestionAsync(int requestId, int userId, string question);

        // Background task
        Task CheckAndUpdateExpiredRequestsAsync();
    }
}
