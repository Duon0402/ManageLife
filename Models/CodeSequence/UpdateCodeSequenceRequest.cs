using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateCodeSequenceRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        public string Category { get; set; } = default!;
        public string Prefix { get; set; } = string.Empty;
        public string? Suffix { get; set; }
        [Range(1, 20)]
        public int NumberLength { get; set; }
    }
}
