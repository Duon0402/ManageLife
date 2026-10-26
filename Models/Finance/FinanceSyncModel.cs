using ManageLife.Commons;

namespace ManageLife.Models
{
    /// <summary>
    /// Kết quả đồng bộ tài chính: các thao tác của app đã xử lý tới đâu, cùng thay đổi ví/danh mục/giao dịch từ mốc app
    /// gửi lên và id những cái đã xoá. Không có mốc (hoặc có thao tác bị từ chối) thì trả toàn bộ.
    /// </summary>
    public class FinanceSyncModel
    {
        /// <summary>Số thao tác đầu đã xử lý (gồm cả bị từ chối): app bỏ chừng ấy thao tác khỏi hàng chờ.</summary>
        public int Processed { get; set; }

        /// <summary>false: dừng giữa chừng vì lỗi server, thao tác còn lại gửi lại sau; không kèm thay đổi.</summary>
        public bool Complete { get; set; }

        /// <summary>Lý do các thao tác bị từ chối (đã bỏ qua, không gửi lại).</summary>
        public List<string> Rejected { get; set; } = new();

        /// <summary>true: là toàn bộ dữ liệu (app thay hết dữ liệu trên máy), false: chỉ thay đổi từ mốc.</summary>
        public bool Full { get; set; }

        /// <summary>Giờ server (UTC) lúc lấy dữ liệu: app gửi lại làm mốc cho lần đồng bộ sau.</summary>
        public DateTime ServerTime { get; set; }

        public List<FinanceWalletModel> Wallets { get; set; } = new();

        public List<FinanceCategoryModel> Categories { get; set; } = new();

        public List<FinanceTransactionModel> Transactions { get; set; } = new();

        public List<FinanceBudgetModel> Budgets { get; set; } = new();

        public List<FinanceRecurringModel> Recurrings { get; set; } = new();

        public List<FinanceDebtModel> Debts { get; set; } = new();

        public List<string> DeletedWalletIds { get; set; } = new();

        public List<string> DeletedCategoryIds { get; set; } = new();

        public List<string> DeletedTransactionIds { get; set; } = new();

        public List<string> DeletedBudgetIds { get; set; } = new();

        public List<string> DeletedRecurringIds { get; set; } = new();

        public List<string> DeletedDebtIds { get; set; } = new();
    }

    public class FinanceWalletModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public FinanceWalletType Type { get; set; }
        public string Icon { get; set; } = null!;
        public string Color { get; set; } = null!;
        public long InitialBalance { get; set; }
        public int SortOrder { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDefault { get; set; }
        /// <summary>Key của ví tạo sẵn ("cash"); null = tự tạo. App ghép ví "Tiền mặt" của khách khi gộp dữ liệu.</summary>
        public string? DefaultKey { get; set; }
    }

    public class FinanceCategoryModel
    {
        public string Id { get; set; } = null!;
        public FinanceCategoryKind Kind { get; set; }
        public string Name { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public string Color { get; set; } = null!;
        public string? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsArchived { get; set; }
        /// <summary>Key của bản tạo sẵn (vd "food") — app khớp từ khoá nhập nhanh; null = tự tạo.</summary>
        public string? DefaultKey { get; set; }
    }

    public class FinanceTransactionModel
    {
        public string Id { get; set; } = null!;
        public FinanceTransactionKind Kind { get; set; }
        public long Amount { get; set; }
        public string WalletId { get; set; } = null!;
        public string? ToWalletId { get; set; }
        public string? CategoryId { get; set; }
        public string? DebtId { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class FinanceDebtModel
    {
        public string Id { get; set; } = null!;
        public byte Direction { get; set; }
        public string PersonName { get; set; } = null!;
        public long Amount { get; set; }
        public string WalletId { get; set; } = null!;
        public DateTime OccurredAt { get; set; }
        public DateOnly? DueDate { get; set; }
        public string? Note { get; set; }
        public bool IsClosed { get; set; }
    }

    public class FinanceRecurringModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public FinanceTransactionKind Kind { get; set; }
        public long Amount { get; set; }
        public string WalletId { get; set; } = null!;
        public string? ToWalletId { get; set; }
        public string? CategoryId { get; set; }
        public string? Note { get; set; }
        public TodoRepeatFrequency RepeatFrequency { get; set; }
        public int RepeatInterval { get; set; }
        public int? RepeatWeekdays { get; set; }
        public int? RepeatMonthDay { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateOnly? NextDate { get; set; }
        public bool IsPaused { get; set; }
    }

    public class FinanceBudgetModel
    {
        public string Id { get; set; } = null!;
        /// <summary>Danh mục chi; null = hạn mức tổng cả kỳ.</summary>
        public string? CategoryId { get; set; }
        public byte PeriodKind { get; set; }
        public DateOnly PeriodStart { get; set; }
        public DateOnly PeriodEnd { get; set; }
        public long Amount { get; set; }
    }
}
