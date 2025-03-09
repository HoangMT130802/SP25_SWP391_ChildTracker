using BusinessLogic.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.ConsultationResponse
{
    public class ConsultationResponseDTO
    {
        public int ResponseId { get; set; }
        public int RequestId { get; set; }
        public int? DoctorId { get; set; }
        public string Response { get; set; }
        public string Attachments { get; set; }
        public bool IsFromUser { get; set; }
        public bool IsQuestion { get; set; }
        public int? ParentResponseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual UserDTO Doctor { get; set; }
        public virtual ConsultationResponseDTO ParentResponse { get; set; }
        public virtual List<ConsultationResponseDTO> ChildResponses { get; set; }
    }
}
