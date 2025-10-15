using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateToDoListRequest
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
