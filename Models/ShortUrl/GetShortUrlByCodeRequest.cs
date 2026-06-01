using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetShortUrlByCodeRequest : IValidatableRequest
    {
        [Required]
        public string Code { get; set; } = default!;
    }
}
