using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateHabitRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "Tên habit không được để trống")]
        [MaxLength(200)]
        public string Name { get; set; } = default!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
