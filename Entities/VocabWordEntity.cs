using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    public class VocabWordEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Word { get; set; } = default!;
        public string? Phonetic { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? Definition { get; set; }
        public string? ExampleSentence { get; set; }
        public string? Transaltion { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public VocabDictionarySource DictionarySource { get; set; } = VocabDictionarySource.Manual;
        public string? RawDictionaryData { get; set; }
        public string OwnerId { get; set; } = default!;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
