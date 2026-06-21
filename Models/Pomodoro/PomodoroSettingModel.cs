namespace ManageLife.Models
{
    public class PomodoroSettingModel
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string? YoutubeUrl { get; set; }
        public string? BackgroundFileId { get; set; }
        public int FocusMinutes { get; set; }
        public int ShortBreakMinutes { get; set; }
        public int LongBreakMinutes { get; set; }
        public int? SessionLoops { get; set; }
    }
}
