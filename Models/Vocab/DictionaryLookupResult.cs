namespace ManageLife.Models
{
    public class DictionaryLookupResult
    {
        public string Word { get; set; } = default!;
        public string? Phonetic { get; set; }
        public string? AudioUrl { get; set; }
        public string? RawJson { get; set; }
        public List<DictionaryMeaningResult> Meanings { get; set; } = [];
    }

    public class DictionaryMeaningResult
    {
        public string PartOfSpeech { get; set; } = default!;
        public string Definition { get; set; } = default!;
        public string? ExampleSentence { get; set; }
    }
}
