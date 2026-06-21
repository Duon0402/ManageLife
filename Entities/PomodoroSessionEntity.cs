using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    public class PomodoroSessionEntity : EntityBase, ICanCreate
    {
        public string UserId { get; set; } = default!;
        public DateTime StartedAt { get; set; }
        public int DurationMinutes { get; set; }
        public PomodoroSessionType Type { get; set; }
        public bool IsCompleted { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
    }
}
