using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateNoteRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
        [Required]
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public List<string> TagIds { get; set; } = [];
    }
}
