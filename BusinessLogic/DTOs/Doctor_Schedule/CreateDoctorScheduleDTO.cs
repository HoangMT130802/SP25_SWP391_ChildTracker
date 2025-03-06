using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BusinessLogic.Utils;

namespace BusinessLogic.DTOs.Doctor_Schedule
{
    public class CreateDoctorScheduleDTO
    {
        [Required(ErrorMessage = "ID bác sĩ không được để trống")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Ngày làm việc phải có định dạng yyyy-MM-dd")]
        public string WorkDate { get; set; }

        [Required(ErrorMessage = "Phải chọn các slot làm việc")]
        [MinLength(6, ErrorMessage = "Phải chọn ít nhất 6 slot làm việc")]
        public List<int> SelectedSlotIds { get; set; }
    }
} 