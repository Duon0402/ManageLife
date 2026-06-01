using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateNoteTagRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        public string Name { get; set; } = default!;
        [Required]
        public string Color { get; set; } = default!;
    }
}
