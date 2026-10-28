using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>
    /// Mục tiêu tiết kiệm: để dành cho 1 việc (số cần, hạn, góp dự kiến mỗi tháng). Tiền không rời ví — mỗi lần góp/rút là
    /// 1 <see cref="FinanceSavingGoalEntryEntity"/> "dành ra" một phần số dư của 1 ví, không phải giao dịch, không tính thu/chi.
    /// </summary>
    public class FinanceSavingGoalEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        public string Name { get; set; } = default!;

        /// <summary>Số tiền cần (&gt; 0).</summary>
        public long TargetAmount { get; set; }

        /// <summary>Hạn (giờ VN); null = không hạn.</summary>
        public DateOnly? Deadline { get; set; }

        /// <summary>Góp dự kiến mỗi tháng; null = không đặt.</summary>
        public long? MonthlyPlan { get; set; }

        public string Icon { get; set; } = default!;

        /// <summary>#RRGGBB</summary>
        public string Color { get; set; } = default!;

        public string? Note { get; set; }

        /// <summary>Đã xong (hoàn thành / dừng): phần đã dành không còn giữ trong ví.</summary>
        public bool IsClosed { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
