using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateVocabDeckRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? TopicId { get; set; }
    }
}
