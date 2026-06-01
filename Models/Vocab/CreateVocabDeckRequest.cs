using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateVocabDeckRequest : IValidatableRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? TopicId { get; set; }
    }
}
