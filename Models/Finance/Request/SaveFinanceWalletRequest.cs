using ManageLife.Commons;
using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceWalletRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID) để tạo khi mất mạng rồi đồng bộ sau; gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        [Required(ErrorMessage = "Tên ví không được để trống")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Tên ví từ 1 đến 50 ký tự")]
        public string Name { get; set; } = null!;

        [EnumDataType(typeof(FinanceWalletType), ErrorMessage = "Loại ví không hợp lệ")]
        public FinanceWalletType Type { get; set; }

        [Required(ErrorMessage = "Thiếu icon ví")]
        [StringLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        public string Icon { get; set; } = null!;

        [Required(ErrorMessage = "Thiếu màu ví")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Màu không hợp lệ (dạng #RRGGBB)")]
        public string Color { get; set; } = "#2563EB";

        [Range(-1_000_000_000_000, 1_000_000_000_000, ErrorMessage = "Số dư ban đầu không hợp lệ")]
        public long InitialBalance { get; set; }

        public bool IsArchived { get; set; }

        /// <summary>Đặt làm ví mặc định (tự bỏ mặc định của ví khác).</summary>
        public bool IsDefault { get; set; }
    }
}
