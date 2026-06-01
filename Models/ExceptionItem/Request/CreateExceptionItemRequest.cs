using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateExceptionItemRequest : IValidatableRequest
    {
        [Required]
        public string Type { get; set; } = null!;
        [Required]
        public string Value { get; set; } = null!;
        public string? Description { get; set; }
    }
}
