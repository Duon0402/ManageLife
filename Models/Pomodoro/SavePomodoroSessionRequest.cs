using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class SavePomodoroSessionRequest : IValidatableRequest
    {
        public PomodoroSessionType Type { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime StartedAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}
