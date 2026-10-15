using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class VerifyResetCodeRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string UserName { get; set; } = null!;

        /// <summary>Mã 6 số gửi qua Telegram.</summary>
        [Required(ErrorMessage = "Mã không được để trống")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "Mã gồm 6 chữ số")]
        public string Code { get; set; } = null!;
    }
}
