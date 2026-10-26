using ManageLife.Core;

namespace ManageLife.Entities
{
    /// <summary>
    /// Khoản vay: mình cho vay (Direction 0) hoặc mình đi vay (1). Tạo khoản vay kèm 1 giao dịch DebtOpen (tiền ra/vào ví lúc
    /// vay), mỗi lần trả là 1 giao dịch DebtPayment — số tiền giao dịch có dấu theo chiều tiền vào/ra ví, không tính thu/chi.
    /// </summary>
    public class FinanceDebtEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        /// <summary>0 = mình cho vay, 1 = mình đi vay.</summary>
        public byte Direction { get; set; }

        public string PersonName { get; set; } = default!;

        /// <summary>Số tiền gốc (&gt; 0).</summary>
        public long Amount { get; set; }

        /// <summary>Ví tiền đi (cho vay) / tiền về (đi vay) lúc tạo.</summary>
        public string WalletId { get; set; } = default!;

        /// <summary>Thời điểm vay (UTC).</summary>
        public DateTime OccurredAt { get; set; }

        /// <summary>Hạn trả (giờ VN); null = không hạn.</summary>
        public DateOnly? DueDate { get; set; }

        public string? Note { get; set; }

        /// <summary>Đã trả đủ (app tự đóng khi tổng trả = gốc).</summary>
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
