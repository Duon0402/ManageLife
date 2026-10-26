using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>
    /// Khoản định kỳ (tiền nhà, điện, lương…): mẫu giao dịch lặp. Không tự ghi — tới ngày app nhắc, người dùng bấm "Ghi"
    /// (sửa được số tiền) hoặc "Bỏ qua kỳ này"; cả 2 đều đẩy NextDate sang kỳ sau (app tính, BE chỉ kiểm hợp lệ).
    /// </summary>
    public class FinanceRecurringEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;
        public string Name { get; set; } = default!;

        /// <summary>Chỉ chi/thu/chuyển.</summary>
        public FinanceTransactionKind Kind { get; set; }

        /// <summary>Số tiền gợi ý mỗi kỳ (&gt; 0); lúc ghi sửa được.</summary>
        public long Amount { get; set; }

        public string WalletId { get; set; } = default!;
        public string? ToWalletId { get; set; }
        public string? CategoryId { get; set; }
        public string? Note { get; set; }

        // Lịch lặp: cùng kiểu với việc lặp (TodoRepeatHelper)
        public TodoRepeatFrequency RepeatFrequency { get; set; }
        public int RepeatInterval { get; set; } = 1;
        /// <summary>Chỉ khi lặp hằng tuần: bitmask T2=1, T3=2, ... CN=64.</summary>
        public byte? RepeatWeekdays { get; set; }
        /// <summary>Lặp hằng tháng/năm: ngày gốc (1–31) để 31/1 → 28/2 → 31/3 thay vì trôi về 28.</summary>
        public byte? RepeatMonthDay { get; set; }

        /// <summary>Kỳ đầu tiên (giờ VN).</summary>
        public DateOnly StartDate { get; set; }

        /// <summary>Lặp đến hết ngày này; null = không giới hạn.</summary>
        public DateOnly? EndDate { get; set; }

        /// <summary>Kỳ chưa ghi kế tiếp; null = đã hết lịch.</summary>
        public DateOnly? NextDate { get; set; }

        public bool IsPaused { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
