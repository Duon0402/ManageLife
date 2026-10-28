using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceSavingGoalRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID); gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        [Required(ErrorMessage = "Nhập tên mục tiêu")]
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
        public string Name { get; set; } = null!;

        [Range(1, 1_000_000_000_000, ErrorMessage = "Số tiền không hợp lệ")]
        public long TargetAmount { get; set; }

        public DateOnly? Deadline { get; set; }

        [Range(1, 1_000_000_000_000, ErrorMessage = "Số tiền góp mỗi tháng không hợp lệ")]
        public long? MonthlyPlan { get; set; }

        [Required(ErrorMessage = "Thiếu icon mục tiêu")]
        [StringLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        public string Icon { get; set; } = null!;

        [Required(ErrorMessage = "Thiếu màu mục tiêu")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Màu không hợp lệ (dạng #RRGGBB)")]
        public string Color { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Note { get; set; }

        public bool IsClosed { get; set; }
    }
}
