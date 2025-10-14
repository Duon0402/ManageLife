using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class CreateToDoListRequest
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
