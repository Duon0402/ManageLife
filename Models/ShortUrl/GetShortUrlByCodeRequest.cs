using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetShortUrlByCodeRequest
    {
        [Required]
        public string Code { get; set; } = default!;
    }
}
