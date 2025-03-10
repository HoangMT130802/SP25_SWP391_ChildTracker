using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.ConsultationResponse
{
    public class DoctorResponseDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập nội dung câu trả lời")]
        public string Answer { get; set; }

        public string Attachments { get; set; }

        [Required(ErrorMessage = "Vui lòng chỉ định câu hỏi cần trả lời")]
        public int QuestionId { get; set; }
    }
} 