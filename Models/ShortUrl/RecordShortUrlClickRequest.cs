using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class RecordShortUrlClickRequest : IValidatableRequest
    {
        [Required]
        public string Code { get; set; } = default!;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referer { get; set; }
    }
}
