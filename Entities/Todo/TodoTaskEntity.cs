using ManageLife.Base;
using ManageLife.Commons;

namespace ManageLife.Entities
{
    public class TodoTaskEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        // TODO: Cải tiến cho phép lặp lại, ...

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public TodoStatus Status { get; set; } = TodoStatus.Pending;
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        // Liên kết về danh sách
        public string TodoListId { get; set; } = null!;
        public TodoListEntity? TodoList { get; set; }

        // Tự liên kết để tạo Subtask
        public string? ParentTaskId { get; set; }
        public TodoTaskEntity? ParentTask { get; set; }
        public ICollection<TodoTaskEntity> SubTasks { get; set; } = new List<TodoTaskEntity>();

        public string CreatedUser { get; set; } = null!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
