using System.ComponentModel.DataAnnotations;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetLanguageByCodeRequest : IValidatableRequest
    {
        [Required]
        public string LanguageCode { get; set; } = null!;
    }
}
