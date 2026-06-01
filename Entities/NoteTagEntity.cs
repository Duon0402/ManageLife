using ManageLife.Core;

namespace ManageLife.Entities
{
    public class NoteTagEntity : EntityBase, ICanCreate, ISoftDelete
    {
        public string Name { get; set; } = default!;
        public string Color { get; set; } = "#6c757d";
        public string OwnerId { get; set; } = default!;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
