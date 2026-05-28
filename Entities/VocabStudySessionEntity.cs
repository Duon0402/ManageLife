using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    public class VocabStudySessionEntity : EntityBase, ICanCreate
    {
        public string UserId { get; set; } = default!;
        public string? DeckId { get; set; }
        public VocabStudyMode StudyMode { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int TotalCards { get; set; }
        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }
        public int SkippedCount { get; set; }
        public int DurationSeconds { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
    }
}
