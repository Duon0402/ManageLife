using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeviceCredentialRequest : IValidatableRequest
    {
        /// <summary>UUID do app tạo, 1–64 ký tự chữ/số/gạch ngang.</summary>
        [Required(ErrorMessage = "Thiếu mã thiết bị")]
        [RegularExpression("^[A-Za-z0-9-]{1,64}$", ErrorMessage = "Mã thiết bị không hợp lệ")]
        public string DeviceId { get; set; } = null!;

        /// <summary>Bí mật base64url của 32 byte (43 ký tự).</summary>
        [Required(ErrorMessage = "Thiếu khóa bảo mật")]
        [RegularExpression("^[A-Za-z0-9_-]{43}$", ErrorMessage = "Khóa bảo mật không hợp lệ")]
        public string Secret { get; set; } = null!;
    }
}
