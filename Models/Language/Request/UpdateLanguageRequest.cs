using System.ComponentModel.DataAnnotations;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class UpdateLanguageRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Code { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
    }
}
