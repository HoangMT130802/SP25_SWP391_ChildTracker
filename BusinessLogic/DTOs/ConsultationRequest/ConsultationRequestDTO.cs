using BusinessLogic.DTOs.Children;
using BusinessLogic.DTOs.ConsultationResponse;
using BusinessLogic.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.ConsultationRequest
{
    public class ConsultationRequestDTO
    {
        public int RequestId { get; set; }
        public int UserId { get; set; }
        public int ChildId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public virtual ChildDTO Child { get; set; }
        public virtual UserDTO User { get; set; }
        public virtual List<ConsultationResponseDTO> Responses { get; set; }
    }
}
