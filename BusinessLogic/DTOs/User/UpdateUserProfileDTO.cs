using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs.User
{
    public class UpdateUserProfileDTO
    {


        [Required(ErrorMessage = "Tên không được để trống")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string Address { get; set; }
      
    }
}
