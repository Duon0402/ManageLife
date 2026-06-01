using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class RemoveNoteLinkRequest : IValidatableRequest
    {
        [Required]
        public string SourceNoteId { get; set; } = default!;
        [Required]
        public string TargetNoteId { get; set; } = default!;
    }
}
