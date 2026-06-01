using ManageLife.Core;

namespace ManageLife.Entities
{
    public class NoteLinkEntity : ICanCreate
    {
        public string SourceNoteId { get; set; } = default!;
        public string TargetNoteId { get; set; } = default!;
        public string OwnerId { get; set; } = default!;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
    }
}
