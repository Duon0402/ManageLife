using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    public class VocabStudyProgressEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string UserId { get; set; } = default!;
        public string WordId { get; set; } = default!;
        public string? DeckId { get; set; }
        public int Repetitions { get; set; }
        public double EasinessFactor { get; set; }
        public int IntervalDays { get; set; }
        public DateTime NextReviewDate { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public int? LastQuality { get; set; }
        public int TotalReviews { get; set; }
        public int CorrectCount { get; set; }
        public int StreakCount { get; set; }
        public VocabMasteryLevel MasteryLevel { get; set; } = VocabMasteryLevel.New;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
