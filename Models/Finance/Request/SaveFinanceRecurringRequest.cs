using ManageLife.Commons;
using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveFinanceRecurringRequest : IValidatableRequest
    {
        /// <summary>Id do app tạo (GUID) để tạo khi mất mạng rồi đồng bộ sau; gửi lại cùng id thì không tạo trùng.</summary>
        public string? Id { get; set; }

        [Required(ErrorMessage = "Nhập tên khoản định kỳ")]
        [StringLength(100, ErrorMessage = "Tên tối đa 100 ký tự")]
        public string Name { get; set; } = null!;

        /// <summary>Chỉ 0 chi, 1 thu, 2 chuyển.</summary>
        [EnumDataType(typeof(FinanceTransactionKind), ErrorMessage = "Loại giao dịch không hợp lệ")]
        public FinanceTransactionKind Kind { get; set; }

        [Range(1, 1_000_000_000_000, ErrorMessage = "Số tiền không hợp lệ")]
        public long Amount { get; set; }

        [Required(ErrorMessage = "Chọn ví")]
        [StringLength(255, ErrorMessage = "Id ví không hợp lệ")]
        public string WalletId { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Id ví không hợp lệ")]
        public string? ToWalletId { get; set; }

        [StringLength(255, ErrorMessage = "Id danh mục không hợp lệ")]
        public string? CategoryId { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        public string? Note { get; set; }

        /// <summary>0 hằng ngày, 1 hằng tuần, 2 hằng tháng, 3 hằng năm.</summary>
        [EnumDataType(typeof(TodoRepeatFrequency), ErrorMessage = "Kiểu lặp không hợp lệ")]
        public TodoRepeatFrequency RepeatFrequency { get; set; }

        [Range(1, 99, ErrorMessage = "Chu kỳ lặp phải từ 1 đến 99")]
        public int RepeatInterval { get; set; } = 1;

        /// <summary>Chỉ khi lặp hằng tuần: bitmask T2=1 ... CN=64.</summary>
        [Range(1, 127, ErrorMessage = "Ngày trong tuần không hợp lệ")]
        public int? RepeatWeekdays { get; set; }

        /// <summary>Lặp hằng tháng/năm: ngày gốc 1–31.</summary>
        [Range(1, 31, ErrorMessage = "Ngày trong tháng không hợp lệ")]
        public int? RepeatMonthDay { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        /// <summary>Kỳ chưa ghi kế tiếp (app tính); null = hết lịch.</summary>
        public DateOnly? NextDate { get; set; }

        public bool IsPaused { get; set; }
    }
}
