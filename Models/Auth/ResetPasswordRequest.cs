using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class ResetPasswordRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; } = null!;

        /// <summary>Mã 6 số gửi qua Telegram.</summary>
        [Required(ErrorMessage = "Mã không được để trống")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã gồm 6 chữ số")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()\-=+\[\]{};:'"",.<>?/\\|`~]).{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ số, 1 chữ hoa, 1 chữ thường và 1 ký tự đặc biệt")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
