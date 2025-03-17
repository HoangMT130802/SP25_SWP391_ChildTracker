using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.DTOs.Doctor
{
    public class CreateDoctorDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chuyên khoa")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập bằng cấp/chứng chỉ")]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số giấy phép hành nghề")]
        public string LicenseNumber { get; set; }


        public int Experience { get; set; }
        public string Biography { get; set; }
    }
}
