using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteVocabWordRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
