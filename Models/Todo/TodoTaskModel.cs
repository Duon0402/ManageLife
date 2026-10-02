using ManageLife.Commons;

namespace ManageLife.Models
{
    public class TodoTaskModel
    {
        public string Id { get; set; } = null!;
        public string? ListId { get; set; }
        public string? ListName { get; set; }
        public string? ListColor { get; set; }
        public string Title { get; set; } = null!;
        public string? Note { get; set; }
        public TodoPriority Priority { get; set; }
        public DateOnly? DueDate { get; set; }
        public TimeOnly? DueTime { get; set; }

        /// <summary>UTC.</summary>
        public DateTime? ReminderAt { get; set; }

        /// <summary>UTC; khác null = đã xong.</summary>
        public DateTime? CompletedAt { get; set; }

        public int SortOrder { get; set; }
        public int ChecklistTotal { get; set; }
        public int ChecklistDone { get; set; }

        /// <summary>UTC.</summary>
        public DateTime CreatedTime { get; set; }
    }

    public class TodoTaskDetailModel : TodoTaskModel
    {
        public List<TodoChecklistItemModel> Checklist { get; set; } = new();
    }
}
