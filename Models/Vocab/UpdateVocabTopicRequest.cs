using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateVocabTopicRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        [MaxLength(7)]
        public string? Color { get; set; }
        [MaxLength(50)]
        public string? Icon { get; set; }
        public bool IsPublic { get; set; }
    }
}
