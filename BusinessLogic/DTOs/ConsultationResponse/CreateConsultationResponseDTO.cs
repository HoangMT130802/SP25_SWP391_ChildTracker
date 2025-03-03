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
        [Required]
        public int RequestId { get; set; }

        [Required]
        [MinLength(10)]
        public string Response { get; set; }

        public string Attachments { get; set; }
    }
}
