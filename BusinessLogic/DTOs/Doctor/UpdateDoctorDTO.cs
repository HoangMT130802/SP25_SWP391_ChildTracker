using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Doctor
{
    public class UpdateDoctorDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chuyên khoa")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập bằng cấp/chứng chỉ")]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số giấy phép hành nghề")]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nơi làm việc")]   
        public int Experience { get; set; }
        public string Biography { get; set; }
        public bool IsVerified { get; set; }  
    }
}
