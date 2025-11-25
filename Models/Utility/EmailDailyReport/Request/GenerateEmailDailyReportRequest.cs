using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GenerateEmailDailyReportRequest
    {
        [Required(ErrorMessage = "Kết quả công việc hôm nay không được để trống")]
        public string TodayResult { get; set; } = null!;

        [Required(ErrorMessage = "Dự kiến công việc ngày mai không được để trống")]
        public string TomorrowPlan { get; set; } = null!;

        public string? Suggestion { get; set; }
    }
}
