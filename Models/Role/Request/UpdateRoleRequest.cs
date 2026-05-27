using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateRoleRequest
    {
        [Required(ErrorMessage = "Id is required")]
        public string Id { get; set; } = null!;
        [Required(ErrorMessage = "Code is required")]
        public string Code { get; set; } = null!;
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
