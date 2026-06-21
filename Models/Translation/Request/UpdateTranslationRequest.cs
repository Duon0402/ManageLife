using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateTranslationRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Key { get; set; } = null!;
        [Required]
        public string Value { get; set; } = null!;
        [Required]
        public string LanguageId { get; set; } = null!;
    }
}
