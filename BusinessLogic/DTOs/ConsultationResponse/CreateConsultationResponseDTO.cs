using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.ConsultationResponse
{
    public class CreateConsultationResponseDTO
    {
        [Required(ErrorMessage = "Vui lòng chọn yêu cầu tư vấn")]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi")]
        public string Response { get; set; }

        public string Attachments { get; set; }

        public bool IsQuestion { get; set; }

        public int? ParentResponseId { get; set; }
    }
}
