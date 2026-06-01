using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateShortUrlRequest : IValidatableRequest
    {
        [Required]
        [Url]
        public string OriginalUrl { get; set; } = default!;

        // null = để service tự scrape hoặc bỏ trống
        public string? Title { get; set; }

        // null = không hết hạn
        public DateTime? ExpireAt { get; set; }
    }

}
