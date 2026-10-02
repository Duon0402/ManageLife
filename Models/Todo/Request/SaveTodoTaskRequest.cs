using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class SaveTodoTaskRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(500, ErrorMessage = "Tiêu đề tối đa 500 ký tự")]
        public string Title { get; set; } = null!;

        [MaxLength(10000, ErrorMessage = "Ghi chú tối đa 10000 ký tự")]
        public string? Note { get; set; }

        /// <summary>null hoặc rỗng = Inbox.</summary>
        public string? ListId { get; set; }

        [Range(0, 3, ErrorMessage = "Độ ưu tiên không hợp lệ")]
        public int Priority { get; set; }

        public DateOnly? DueDate { get; set; }
        public TimeOnly? DueTime { get; set; }
        public DateTime? ReminderAt { get; set; }

        /// <summary>Tiêu đề các mục checklist, chỉ dùng khi tạo task (độ dài từng mục kiểm ở service).</summary>
        [MaxLength(100, ErrorMessage = "Checklist tối đa 100 mục")]
        public List<string>? Checklist { get; set; }
    }
}
