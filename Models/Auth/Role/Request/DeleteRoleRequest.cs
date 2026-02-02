using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteRoleRequest
    {
        [Required(ErrorMessage = "RoleId is required")]
        public string RoleId { get; set; } = null!;
    }
}
