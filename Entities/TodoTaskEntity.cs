using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Entities
{
    public class TodoTaskEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public string OwnerId { get; set; } = default!;

        /// <summary>null = Inbox.</summary>
        public string? ListId { get; set; }

        public string Title { get; set; } = default!;
        public string? Note { get; set; }
        public TodoPriority Priority { get; set; } = TodoPriority.None;

        /// <summary>Ngày hết hạn theo giờ VN (giờ treo tường, không quy đổi múi giờ).</summary>
        public DateOnly? DueDate { get; set; }

        /// <summary>Giờ hết hạn theo giờ VN; null = cả ngày.</summary>
        public TimeOnly? DueTime { get; set; }

        /// <summary>Thời điểm nhắc (UTC).</summary>
        public DateTime? ReminderAt { get; set; }

        /// <summary>Thời điểm hoàn thành (UTC); khác null = đã xong.</summary>
        public DateTime? CompletedAt { get; set; }

        public int SortOrder { get; set; }

        // Lặp lại: tick xong sẽ tạo lần kế tiếp (TodoTaskService.SpawnNextOccurrenceAsync)
        public TodoRepeatFrequency? RepeatFrequency { get; set; }
        public int? RepeatInterval { get; set; }
        /// <summary>Bitmask thứ trong tuần: T2=1, T3=2, ... CN=64.</summary>
        public byte? RepeatWeekdays { get; set; }
        public DateOnly? RepeatUntil { get; set; }

        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
