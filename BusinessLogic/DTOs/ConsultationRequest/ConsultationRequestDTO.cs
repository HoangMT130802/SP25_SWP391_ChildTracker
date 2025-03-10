using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.DTOs.User;
using System;
using System.Collections.Generic;

namespace BusinessLogic.DTOs.ConsultationRequest
{
    public class ConsultationRequestDTO
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public int ChildId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public bool IsSatisfied { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedReason { get; set; }
        
        public BaseUserDTO User { get; set; }
        public BaseUserDTO AssignedDoctor { get; set; }
        public ICollection<ConsultationResponseDTO> ConsultationResponses { get; set; }
    }
}
