using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateVocabWordRequest : IValidatableRequest
    {
        [Required]
        [MaxLength(100)]
        public string Word { get; set; } = default!;

        [Required]
        [MaxLength(1000)]
        public string Definition { get; set; } = default!;

        [MaxLength(100)]
        public string? Phonetic { get; set; }

        [MaxLength(50)]
        public string? PartOfSpeech { get; set; }

        [MaxLength(500)]
        public string? ExampleSentence { get; set; }

        [MaxLength(500)]
        public string? Translation { get; set; }

        [MaxLength(500)]
        public string? AudioUrl { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public string? RawDictionaryData { get; set; }

        public int DictionarySource { get; set; }
    }
}
