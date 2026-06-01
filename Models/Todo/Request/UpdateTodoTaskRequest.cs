using ManageLife.Commons;
using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateTodoTaskRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "Id không được để trống")]
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Danh sách không được để trống")]
        public string TodoListId { get; set; } = null!;

        public string? ParentTaskId { get; set; }

        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? EstimatedMinutes { get; set; }

        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
        public DateTime? RecurrenceEndDate { get; set; }

        public DateTime? ReminderAt { get; set; }
    }
}
