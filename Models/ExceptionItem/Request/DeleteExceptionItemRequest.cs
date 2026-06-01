using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteExceptionItemRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
