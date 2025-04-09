using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Appointment
{
    public class CompleteAppointmentDTO
    {
        [Required(ErrorMessage = "Ghi chú không được để trống.")]
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự.")]
        public string Note { get; set; }
    }
}
