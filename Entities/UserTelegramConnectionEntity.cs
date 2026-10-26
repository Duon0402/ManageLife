using ManageLife.Core;

namespace ManageLife.Entities
{
    public class UserTelegramConnectionEntity : EntityBase, ICanCreate, ICanUpdate, ISoftDelete
    {
        public long ChatId { get; set; } = default!;
        public string UserId { get; set; } = default!;

        /// <summary>Có gửi tóm tắt buổi sáng (công việc + tài chính) qua chat này không.</summary>
        public bool DailySummaryEnabled { get; set; } = true;

        /// <summary>Giờ gửi tóm tắt theo giờ VN (0–23); cron chạy mỗi giờ, chỉ gửi cho người chọn đúng giờ đó.</summary>
        public byte DailySummaryHour { get; set; } = 7;
        public string CreatedUser { get; set; } = default!;
        public DateTime CreatedTime { get; set; }
        public string? UpdatedUser { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? DeletedUser { get; set; }
        public DateTime? DeletedTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
