using ManageLife.Core;

namespace ManageLife.Entities
{
    public class TranslationEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
        public string LanguageId { get; set; } = default!;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
