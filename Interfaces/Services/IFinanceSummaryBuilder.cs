namespace ManageLife.Interfaces
{
    public interface IFinanceSummaryBuilder
    {
        /// <summary>
        /// Đoạn "💰 Tài chính" của tin tóm tắt buổi sáng cho từng người dùng (key = userId). Người không có gì đáng báo (không
        /// giao dịch 31 ngày qua, không khoản định kỳ cần ghi / khoản vay tới hạn) thì không có trong kết quả.
        /// </summary>
        Task<Dictionary<string, string>> BuildAsync(IReadOnlyCollection<string> userIds, DateOnly today, CancellationToken ct = default);
    }
}
