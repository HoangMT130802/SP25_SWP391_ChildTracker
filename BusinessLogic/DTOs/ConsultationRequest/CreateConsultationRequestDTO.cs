using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.ConsultationRequest
{
    public class CreateConsultationRequestDTO
    {
        [Required(ErrorMessage = "Vui lòng chọn trẻ cần tư vấn")]
        public int ChildId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung cần tư vấn")]
        [MinLength(10, ErrorMessage = "Nội dung tư vấn phải có ít nhất 10 ký tự")]
        public string Description { get; set; }
    }
}
