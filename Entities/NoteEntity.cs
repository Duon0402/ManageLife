using ManageLife.Core;

namespace ManageLife.Entities
{
    public class NoteEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
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
