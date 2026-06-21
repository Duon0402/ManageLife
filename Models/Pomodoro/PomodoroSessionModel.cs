using ManageLife.Commons;

namespace ManageLife.Models
{
    public class PomodoroSessionModel
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public DateTime StartedAt { get; set; }
        public int DurationMinutes { get; set; }
        public PomodoroSessionType Type { get; set; }
        public bool IsCompleted { get; set; }
    }
}
