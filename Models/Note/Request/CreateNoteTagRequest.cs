using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateNoteTagRequest : IValidatableRequest
    {
        [Required]
        public string Name { get; set; } = default!;
        [Required]
        public string Color { get; set; } = "#6c757d";
    }
}
