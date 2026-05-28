using ManageLife.Commons;

namespace ManageLife.Models
{
    public class VocabWordModel
    {
        public string Id { get; set; } = default!;
        public string Word { get; set; } = default!;
        public string? Phonetic { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Definition { get; set; }
        public string? ExampleSentence { get; set; }
        public string? Translation { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public VocabDictionarySource DictionarySource { get; set; }
        public VocabMasteryLevel MasteryLevel { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
