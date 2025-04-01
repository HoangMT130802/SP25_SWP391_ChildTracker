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
        public int ChildId { get; set; }
        public string Description { get; set; }
    }
}
