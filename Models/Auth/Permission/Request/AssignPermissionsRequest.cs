using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class AssignPermissionsRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = null!;
        public List<string> PermissionIds { get; set; } = new List<string>();
    }
}
