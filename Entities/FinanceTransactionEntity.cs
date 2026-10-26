using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>Giao dịch thu/chi/chuyển/điều chỉnh của người dùng.</summary>
    public class FinanceTransactionEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        public FinanceTransactionKind Kind { get; set; }

        /// <summary>Số tiền (đồng, luôn dương trừ Adjust: âm = giảm số dư).</summary>
        public long Amount { get; set; }

        /// <summary>Ví nguồn (chi/điều chỉnh) hoặc ví nhận (thu).</summary>
        public string WalletId { get; set; } = default!;

        /// <summary>Ví đích — chỉ có khi chuyển giữa 2 ví.</summary>
        public string? ToWalletId { get; set; }

        /// <summary>Danh mục thu/chi — không có khi chuyển hoặc điều chỉnh.</summary>
        public string? CategoryId { get; set; }

        /// <summary>Khoản vay của giao dịch DebtOpen/DebtPayment; null với giao dịch thường.</summary>
        public string? DebtId { get; set; }

        /// <summary>Thời điểm phát sinh (UTC).</summary>
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
