using ManageLife.Core;
using ManageLife.Models;
using ManageLife.Models.Pomodoro;

namespace ManageLife.Interfaces
{
    public interface IPomodoroService
    {
        Task<Result> SaveSessionAsync(SavePomodoroSessionRequest request);
        Task<Result> SaveSettingAsync(SavePomodoroSettingRequest request);
    }
}
