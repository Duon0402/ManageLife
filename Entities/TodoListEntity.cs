using ManageLife.Core;

namespace ManageLife.Entities
{
    public class TodoListEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Color { get; set; } = "#4F46E5";
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
