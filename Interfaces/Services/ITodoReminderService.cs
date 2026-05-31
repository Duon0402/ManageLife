using ManageLife.Core;

namespace ManageLife.Interfaces
{
    public interface ITodoReminderService
    {
        Task<Result> ProcessPendingRemindersAsync(CancellationToken ct = default);
        Task<Result> SendDailySummaryAsync(CancellationToken ct = default);
    }
}
