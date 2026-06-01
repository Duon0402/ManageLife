using ManageLife.Core;

namespace ManageLife.Entities
{
    public class CodeSequenceEntity : EntityBase, ICanCreate
    {
        public string Category { get; set; } = default!;
        public string Prefix { get; set; } = string.Empty;
        public string? Suffix { get; set; }
        public int NumberLength { get; set; } = 6;
        public long CurrentSeq { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
    }
}
