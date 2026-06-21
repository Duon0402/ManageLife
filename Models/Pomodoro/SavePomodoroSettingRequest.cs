using ManageLife.Core;

namespace ManageLife.Models.Pomodoro
{
    public class SavePomodoroSettingRequest : IValidatableRequest
    {
        public string? YoutubeUrl { get; set; }
        public string? BackgroundFileId { get; set; }
        public int FocusMinutes { get; set; } = 25;
        public int ShortBreakMinutes { get; set; } = 5;
        public int LongBreakMinutes { get; set; } = 15;
        public int? SessionLoops { get; set; }
    }
}
