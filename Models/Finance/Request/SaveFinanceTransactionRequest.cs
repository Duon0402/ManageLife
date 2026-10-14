using ManageLife.Commons;
using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceTransactionRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID) để tạo khi mất mạng rồi đồng bộ sau; gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        [EnumDataType(typeof(FinanceTransactionKind), ErrorMessage = "Loại giao dịch không hợp lệ")]
        public FinanceTransactionKind Kind { get; set; }

        /// <summary>Số tiền (đồng). Điều chỉnh: âm = giảm số dư.</summary>
        [Range(-1_000_000_000_000, 1_000_000_000_000, ErrorMessage = "Số tiền không hợp lệ")]
        public long Amount { get; set; }

        [Required(ErrorMessage = "Thiếu ví")]
        [StringLength(255, ErrorMessage = "Id ví không hợp lệ")]
        public string WalletId { get; set; } = null!;

        /// <summary>Ví đích — bắt buộc khi chuyển giữa 2 ví.</summary>
        [StringLength(255, ErrorMessage = "Id ví không hợp lệ")]
        public string? ToWalletId { get; set; }

        /// <summary>Danh mục — bắt buộc khi thu/chi.</summary>
        [StringLength(255, ErrorMessage = "Id danh mục không hợp lệ")]
        public string? CategoryId { get; set; }

        [Required(ErrorMessage = "Thiếu thời điểm giao dịch")]
        public DateTime OccurredAt { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Note { get; set; }
    }
}
