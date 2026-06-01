using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models.ShortUrl
{
    public class DeleteShortUrlRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
