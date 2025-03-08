using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Appointment
{
    public class UpdateAppointmentStatusDTO
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string Status { get; set; }
    }
} 