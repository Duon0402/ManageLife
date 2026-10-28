using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceSavingGoalEntryRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID); gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        [Required(ErrorMessage = "Thiếu mục tiêu")]
        [StringLength(255, ErrorMessage = "Id mục tiêu không hợp lệ")]
        public string GoalId { get; set; } = null!;

        [Required(ErrorMessage = "Chọn ví")]
        [StringLength(255, ErrorMessage = "Id ví không hợp lệ")]
        public string WalletId { get; set; } = null!;

        /// <summary>+ góp, − rút (≠ 0).</summary>
        [Range(-1_000_000_000_000, 1_000_000_000_000, ErrorMessage = "Số tiền không hợp lệ")]
        public long Amount { get; set; }

        [Required(ErrorMessage = "Thiếu thời điểm góp")]
        public DateTime OccurredAt { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Note { get; set; }
    }
}
