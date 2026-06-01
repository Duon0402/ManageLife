using System.ComponentModel.DataAnnotations;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class ChangeLanguageRequest : IValidatableRequest
    {
        [Required]
        public string LanguageCode { get; set; } = null!;
        public string? ReturnUrl { get; set; }
    }
}
