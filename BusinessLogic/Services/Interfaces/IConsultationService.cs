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
        Task<ConsultationResponseDTO> AddQuestionAsync(int requestId, int userId, AskQuestionDTO questionDto);
        Task<ConsultationResponseDTO> AddDoctorResponseAsync(int requestId, int doctorId, DoctorResponseDTO responseDto);
        Task<ConsultationRequestDTO> CompleteRequestAsync(int requestId, int userId, bool isSatisfied);
        Task<ConsultationRequestDTO> CloseRequestAsync(int requestId, string reason, string closedBy);
        Task<ConsultationResponseDTO> UpdateResponseAsync(int responseId, string newResponse);
        Task<ConsultationRequestDTO> AssignDoctorAsync(int requestId, int doctorId);
        Task<Dictionary<int, int>> GetDoctorWorkloadAsync();

        // Background task
        Task CheckAndUpdateExpiredRequestsAsync();
    }
}
