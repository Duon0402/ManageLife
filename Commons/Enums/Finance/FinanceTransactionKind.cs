namespace ManageLife.Commons
{
    /// <summary>
    /// Loại giao dịch: chi, thu, chuyển giữa 2 ví (không tính vào thu/chi), điều chỉnh số dư (khớp số thật của ví),
    /// trả nợ / mở khoản vay (thuộc 1 khoản vay, số tiền có dấu theo chiều tiền vào/ra ví, không tính vào thu/chi).
    /// </summary>
    public enum FinanceTransactionKind : byte
    {
        Expense = 0,
        Income = 1,
        Transfer = 2,
        Adjust = 3,
        /// <summary>1 lần trả của khoản vay: được trả (+, mình cho vay) / trả nợ (−, mình đi vay).</summary>
        DebtPayment = 4,
        /// <summary>Lúc tạo khoản vay: cho vay (−) / nhận tiền vay (+).</summary>
        DebtOpen = 5
    }
}
