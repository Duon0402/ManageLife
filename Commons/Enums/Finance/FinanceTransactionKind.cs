namespace ManageLife.Commons
{
    /// <summary>
    /// Loại giao dịch: chi, thu, chuyển giữa 2 ví (không tính vào thu/chi), điều chỉnh số dư (khớp số thật của ví).
    /// </summary>
    public enum FinanceTransactionKind : byte
    {
        Expense = 0,
        Income = 1,
        Transfer = 2,
        Adjust = 3
    }
}
