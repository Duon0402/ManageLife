using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class SavePomodoroSessionRequest : IValidatableRequest
    {
        public PomodoroSessionType Type { get; set; }
        public int DurationMinutes { get; set; }
    }
}
