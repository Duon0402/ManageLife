using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateCodeSequenceRequest : IValidatableRequest
    {
        [Required]
        public string Category { get; set; } = default!;
        public string Prefix { get; set; } = string.Empty;
        public string? Suffix { get; set; }
        [Range(1, 20)]
        public int NumberLength { get; set; } = 6;
    }
}
