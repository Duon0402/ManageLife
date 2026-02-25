using ManageLife.Base;

namespace ManageLife.Entities
{
    public class FolderEntity : EntityBase, ICanCreate, ICanUpdate
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
