using ManageLife.Core;
using ManageLife.Models;
using ManageLife.Models.Pomodoro;

namespace ManageLife.Interfaces
{
    public interface IPomodoroService
    {
        Task<Result> SaveSessionAsync(SavePomodoroSessionRequest request, CancellationToken ct = default);
        Task<Result> SaveSessionsAsync(List<SavePomodoroSessionRequest> requests, CancellationToken ct = default);
        Task<Result> SaveSettingAsync(SavePomodoroSettingRequest request, CancellationToken ct = default);
        Task<Result<PomodoroSettingModel>> GetSettingsAsync(CancellationToken ct = default);
        Task<Result<PomodoroHistoryModel>> GetHistoryAsync(int days = 7, CancellationToken ct = default);
    }
}
