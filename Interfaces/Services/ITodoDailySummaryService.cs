using ManageLife.Core;

namespace ManageLife.Interfaces
{
    public interface ITodoDailySummaryService
    {
        /// <summary>Gửi tóm tắt việc hôm nay + quá hạn qua Telegram cho mọi người dùng đã liên kết.</summary>
        Task<Result> SendDailySummaryAsync(CancellationToken ct = default);
    }
}
