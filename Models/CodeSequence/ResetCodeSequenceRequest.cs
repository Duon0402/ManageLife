using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class ResetCodeSequenceRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
        [Range(0, long.MaxValue)]
        public long Value { get; set; }
    }
}
