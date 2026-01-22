using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UnassignPermissionsRequest
    {
        [Required]
        public string UserId { get; set; } = null!;
        public List<string> PermissionIds { get; set; } = new List<string>();
    }
}
