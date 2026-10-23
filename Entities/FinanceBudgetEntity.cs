using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>Hạn mức chi 1 kỳ: tổng chi cả kỳ hoặc 1 danh mục chi.</summary>
    public class FinanceBudgetEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        /// <summary>Danh mục chi; rỗng = hạn mức tổng cả kỳ (rỗng để tạo được UNIQUE index).</summary>
        public string CategoryId { get; set; } = string.Empty;

        /// <summary>Loại kỳ: 0 tuần, 1 tháng, 2 quý, 3 năm, 4 khoảng tuỳ chọn.</summary>
        public byte PeriodKind { get; set; }

        /// <summary>Ngày đầu kỳ.</summary>
        public DateOnly PeriodStart { get; set; }

        /// <summary>Ngày cuối kỳ.</summary>
        public DateOnly PeriodEnd { get; set; }

        /// <summary>Hạn mức (đồng), luôn dương.</summary>
        public long Amount { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
