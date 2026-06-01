using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateNoteRequest : IValidatableRequest
    {
        [Required]
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public List<string> TagIds { get; set; } = [];
    }
}
