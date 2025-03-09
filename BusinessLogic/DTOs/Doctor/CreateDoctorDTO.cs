using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Doctor
{
    public class CreateDoctorDTO
    {
        [Required(ErrorMessage = "Username không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username phải từ 3-50 ký tự")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-50 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Chuyên khoa không được để trống")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "Bằng cấp không được để trống")]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "Số giấy phép không được để trống")]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "Nơi làm việc không được để trống")]
        public string WorkPlace { get; set; }

        public string Experience { get; set; }
        public string Description { get; set; }
        public string Avatar { get; set; }
    }
}
