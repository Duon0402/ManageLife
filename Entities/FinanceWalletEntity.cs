using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>Ví tiền của người dùng (tiền mặt, ngân hàng, ví điện tử, thẻ tín dụng).</summary>
    public class FinanceWalletEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        public string Name { get; set; } = default!;

        public FinanceWalletType Type { get; set; }

        public string Icon { get; set; } = default!;

        /// <summary>Màu #RRGGBB.</summary>
        public string Color { get; set; } = default!;

        /// <summary>Số dư đầu kỳ (đồng); số dư hiện tại = InitialBalance + thu − chi ± chuyển ± điều chỉnh.</summary>
        public long InitialBalance { get; set; }

        public int SortOrder { get; set; }

        /// <summary>Ẩn ví (vd không dùng nữa) thay vì xoá: ví đã có giao dịch chỉ ẩn được.</summary>
        public bool IsArchived { get; set; }

        /// <summary>Key của bản tạo sẵn (vd "cash"); null = người dùng tự tạo.</summary>
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
