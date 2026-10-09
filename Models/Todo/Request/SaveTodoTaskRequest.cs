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

        /// <summary>null = không lặp. 0 hằng ngày, 1 hằng tuần, 2 hằng tháng, 3 hằng năm (cần có DueDate).</summary>
        [Range(0, 3, ErrorMessage = "Kiểu lặp không hợp lệ")]
        public int? RepeatFrequency { get; set; }

        /// <summary>Mỗi N ngày/tuần/tháng/năm; mặc định 1.</summary>
        [Range(1, 365, ErrorMessage = "Chu kỳ lặp phải từ 1 đến 365")]
        public int? RepeatInterval { get; set; }

        /// <summary>Chỉ khi lặp hằng tuần: bitmask T2=1, T3=2, ... CN=64; null = thứ của ngày hạn.</summary>
        [Range(1, 127, ErrorMessage = "Ngày trong tuần không hợp lệ")]
        public int? RepeatWeekdays { get; set; }

        /// <summary>Lặp đến hết ngày này (giờ VN); null = không giới hạn.</summary>
        public DateOnly? RepeatUntil { get; set; }

        /// <summary>Tiêu đề các mục checklist, chỉ dùng khi tạo task (độ dài từng mục kiểm ở service).</summary>
        [MaxLength(100, ErrorMessage = "Checklist tối đa 100 mục")]
        public List<string>? Checklist { get; set; }
    }
}
