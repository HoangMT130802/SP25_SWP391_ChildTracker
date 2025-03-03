﻿using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.Doctor
{
    public class CreateDoctorDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ không được để trống")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Chuyên khoa không được để trống")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "Bằng cấp không được để trống")]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "Số giấy phép hành nghề không được để trống")]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "Nơi làm việc không được để trống")]
        public string WorkPlace { get; set; }

        public string Experience { get; set; }
        public string Description { get; set; }
        public string Avatar { get; set; }
    }
}
