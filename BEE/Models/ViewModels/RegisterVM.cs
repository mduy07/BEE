using System.ComponentModel.DataAnnotations;

namespace BEE.Models.ViewModel
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập!")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp!")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên!")]
        public string CustomerName { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}