using ManageLife.Core;
using ManageLife.Commons;

namespace ManageLife.Entities
{
    public class TodoTaskEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string Title { get; set; } = default!;
        public string? Description { get; set; }

        public TodoStatus Status { get; set; } = TodoStatus.Pending;
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? EstimatedMinutes { get; set; }

        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
        public DateTime? RecurrenceEndDate { get; set; }

        public DateTime? ReminderAt { get; set; }
        public bool IsReminderSent { get; set; }

        public string TodoListId { get; set; } = default!;
        public string? ParentTaskId { get; set; }
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
