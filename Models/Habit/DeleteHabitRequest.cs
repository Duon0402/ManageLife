using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteHabitRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = default!;
    }
}
