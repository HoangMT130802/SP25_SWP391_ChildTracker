using BusinessLogic.DTOs.User;
using System;

namespace BusinessLogic.DTOs.ConsultationResponse
{
    public class ConsultationResponseDTO
    {
        public int ResponseId { get; set; }
        public int RequestId { get; set; }
        public string Response { get; set; }
        public string Attachments { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFromUser { get; set; }
        public BaseUserDTO Doctor { get; set; }
    }
}
