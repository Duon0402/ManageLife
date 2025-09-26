using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
	public class LoginAccountRequest
	{
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = null!;
    }
}
