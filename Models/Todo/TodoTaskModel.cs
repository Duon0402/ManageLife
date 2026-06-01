using ManageLife.Commons;

namespace ManageLife.Models
{
    public class TodoTaskModel
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public TodoStatus Status { get; set; }
        public TodoPriority Priority { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? EstimatedMinutes { get; set; }

        public RecurrenceType Recurrence { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }

        public DateTime? ReminderAt { get; set; }
        public bool IsReminderSent { get; set; }

        public string TodoListId { get; set; } = null!;
        public string? TodoListName { get; set; }
        public string? ParentTaskId { get; set; }

        public List<TodoTaskModel> SubTasks { get; set; } = [];
    }
}
