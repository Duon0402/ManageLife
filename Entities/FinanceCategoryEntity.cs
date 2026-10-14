using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>Danh mục thu/chi (1 cấp; ParentId chừa cho cấp con của giai đoạn sau).</summary>
    public class FinanceCategoryEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        public FinanceCategoryKind Kind { get; set; }

        public string Name { get; set; } = default!;

        public string Icon { get; set; } = default!;

        /// <summary>Màu #RRGGBB.</summary>
        public string Color { get; set; } = default!;

        /// <summary>Danh mục cha; GĐ1 luôn null (chưa hỗ trợ danh mục con).</summary>
        public string? ParentId { get; set; }

        public int SortOrder { get; set; }

        public bool IsArchived { get; set; }

        /// <summary>Key của bản tạo sẵn (vd "food"); null = người dùng tự tạo.</summary>
        public string? DefaultKey { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
