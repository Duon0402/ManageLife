using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models.ShortUrl
{
    public class DeleteShortUrlRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
