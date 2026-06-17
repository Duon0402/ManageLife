using ManageLife.Core;

namespace ManageLife.Entities
{
    public class PomodoroSettingEntity : EntityBase, ICanCreate, ICanUpdate
    {
        public string UserId { get; set; } = default!;
        public string? YoutubeUrl { get; set; }
        public string? BackgroundFileId { get; set; }
        public int FocusMinutes { get; set; } = 25;
        public int ShortBreakMinutes { get; set; } = 5;
        public int LongBreakMinutes { get; set; } = 15;
        public int? SessionLoops { get; set; }  // null = vô hạn

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }   
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
