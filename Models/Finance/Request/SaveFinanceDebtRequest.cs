using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceDebtRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID); gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        /// <summary>0 = mình cho vay, 1 = mình đi vay.</summary>
        [Range(0, 1, ErrorMessage = "Chiều khoản vay không hợp lệ")]
        public byte Direction { get; set; }

        [Required(ErrorMessage = "Nhập tên người vay / cho vay")]
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
        public string PersonName { get; set; } = null!;

        [Range(1, 1_000_000_000_000, ErrorMessage = "Số tiền không hợp lệ")]
        public long Amount { get; set; }

        [Required(ErrorMessage = "Chọn ví")]
        [StringLength(255, ErrorMessage = "Id ví không hợp lệ")]
        public string WalletId { get; set; } = null!;

        [Required(ErrorMessage = "Thiếu thời điểm vay")]
        public DateTime OccurredAt { get; set; }

        public DateOnly? DueDate { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Note { get; set; }

        public bool IsClosed { get; set; }
    }
}
