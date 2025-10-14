using ManageLife.Base;

namespace ManageLife.Entities
{
    public class TodoListEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<TodoTaskEntity> Tasks { get; set; } = new List<TodoTaskEntity>();

        public string CreatedUser { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
