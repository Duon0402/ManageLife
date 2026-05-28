using ManageLife.Commons;

namespace ManageLife.Models
{
    public class StudyCardModel
    {
        public string WordId { get; set; } = default!;
        public string Word { get; set; } = default!;
        public string? Phonetic { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Definition { get; set; }
        public string? ExampleSentence { get; set; }
        public string? Translation { get; set; }
        public string? AudioUrl { get; set; }
        public int Repetitions { get; set; }
        public int IntervalDays { get; set; }
        public VocabMasteryLevel MasteryLevel { get; set; }
        public bool IsNew { get; set; }
    }

    public class SubmitReviewRequest
    {
        public string WordId { get; set; } = default!;
        public string DeckId { get; set; } = default!;
        public int Quality { get; set; }
    }
}
