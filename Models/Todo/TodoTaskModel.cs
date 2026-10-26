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
        public DateOnly? StartDate { get; set; }
        public TimeOnly? DueTime { get; set; }

        /// <summary>UTC.</summary>
        public DateTime? ReminderAt { get; set; }

        /// <summary>Nhắc kiểu báo thức (đổ chuông, toàn màn hình).</summary>
        public bool ReminderAlarm { get; set; }

        /// <summary>UTC; khác null = đã xong.</summary>
        public DateTime? CompletedAt { get; set; }

        public int SortOrder { get; set; }

        /// <summary>null = không lặp.</summary>
        public TodoRepeatFrequency? RepeatFrequency { get; set; }
        public int? RepeatInterval { get; set; }
        /// <summary>Bitmask T2=1 ... CN=64 (chỉ khi lặp hằng tuần).</summary>
        public int? RepeatWeekdays { get; set; }
        public DateOnly? RepeatUntil { get; set; }
        /// <summary>Lặp tháng/năm: ngày gốc 1–31 (app tính lần kế tiếp không bị trôi 31 → 28).</summary>
        public int? RepeatMonthDay { get; set; }

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
