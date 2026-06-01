using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GenerateEmailDailyReportRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "CurrentBusinessDay is required")]
        public DateTime CurrentBusinessDay { get; set; }

        [Required(ErrorMessage = "NextBusinessDay is required")]
        public DateTime NextBusinessDay { get; set; }

        [Required(ErrorMessage = "TodayWorkResults is required")]
        public string TodayWorkResults { get; set; } = null!;

        [Required(ErrorMessage = "PlannedWorkTomorrow is required")]
        public string PlannedWorkTomorrow { get; set; } = null!;

        public string? Suggestions { get; set; }
    }
}
