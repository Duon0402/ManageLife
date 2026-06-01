using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class LookupWordRequest : IValidatableRequest
    {
        [Required]
        public string Word { get; set; } = default!;
    }
}
