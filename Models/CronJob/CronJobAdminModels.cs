using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    /// <summary>Job cron hiển thị ở trang admin; lịch dạng chuỗi ("*" = mọi, "1,15" = danh sách). Không bao giờ chứa secret.</summary>
    public class CronJobViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = null!;
        public string Url { get; set; } = null!;
        public bool Enabled { get; set; }
        public int RequestMethod { get; set; }
        public string Hours { get; set; } = "*";
        public string Minutes { get; set; } = "*";
        public string Mdays { get; set; } = "*";
        public string Months { get; set; } = "*";
        public string Wdays { get; set; } = "*";
        public string ScheduleText { get; set; } = null!;
        public string Timezone { get; set; } = null!;
        public string? Body { get; set; }
        /// <summary>Job có gắn header X-Cron-Secret (chỉ cho biết có/không).</summary>
        public bool AttachCronSecret { get; set; }
        /// <summary>Job hệ thống khai báo trong code (tạo/cập nhật bằng "Đồng bộ job hệ thống").</summary>
        public bool IsSystem { get; set; }
        public int LastStatus { get; set; }
        public string LastStatusText { get; set; } = null!;
        public string? LastExecution { get; set; }
        public string? NextExecution { get; set; }
    }

    public class SaveCronJobRequest : IValidatableRequest
    {
        /// <summary>0 = tạo mới.</summary>
        public int JobId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "URL không được để trống")]
        [MaxLength(1000, ErrorMessage = "URL tối đa 1000 ký tự")]
        [RegularExpression(@"^https?://\S+$", ErrorMessage = "URL phải bắt đầu bằng http:// hoặc https://")]
        public string Url { get; set; } = null!;

        public bool Enabled { get; set; } = true;

        [Range(0, 8, ErrorMessage = "Phương thức không hợp lệ")]
        public int RequestMethod { get; set; }

        public string? Hours { get; set; }
        public string? Minutes { get; set; }
        public string? Mdays { get; set; }
        public string? Months { get; set; }
        public string? Wdays { get; set; }

        [MaxLength(10000, ErrorMessage = "Body tối đa 10000 ký tự")]
        public string? Body { get; set; }

        /// <summary>Server tự gắn header X-Cron-Secret (giá trị lấy từ cấu hình, không qua client).</summary>
        public bool AttachCronSecret { get; set; }
    }

    public class CronJobIdRequest : IValidatableRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Job không hợp lệ")]
        public int JobId { get; set; }
    }

    public class CronJobHistoryModel
    {
        public string Date { get; set; } = null!;
        public int Duration { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; } = null!;
        public int HttpStatus { get; set; }
    }
}
