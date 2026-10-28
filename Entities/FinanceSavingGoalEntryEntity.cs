using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>1 lần góp (Amount &gt; 0) hoặc rút (Amount &lt; 0) của mục tiêu tiết kiệm, dành ra từ / trả về 1 ví.</summary>
    public class FinanceSavingGoalEntryEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        public string GoalId { get; set; } = default!;

        public string WalletId { get; set; } = default!;

        /// <summary>+ góp, − rút (≠ 0).</summary>
        public long Amount { get; set; }

        /// <summary>Thời điểm góp/rút (UTC).</summary>
        public DateTime OccurredAt { get; set; }

        public string? Note { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
