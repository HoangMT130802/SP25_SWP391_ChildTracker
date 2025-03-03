using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.User
{
    public class UpdateUserProfileDTO
    {
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

        public string Avatar { get; set; }
    }
}
