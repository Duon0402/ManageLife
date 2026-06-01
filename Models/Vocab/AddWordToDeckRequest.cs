using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class AddWordToDeckRequest : IValidatableRequest
    {
        [Required]
        public string DeckId { get; set; } = default!;
        [Required]
        public string WordId { get; set; } = default!;
    }
}
