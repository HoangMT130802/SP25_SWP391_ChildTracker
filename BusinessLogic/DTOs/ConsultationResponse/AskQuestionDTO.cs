using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.ConsultationResponse
{
    public class AskQuestionDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        public string Question { get; set; }

        public string? Attachments { get; set; }
    }
} 