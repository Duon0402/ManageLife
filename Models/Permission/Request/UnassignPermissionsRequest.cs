using ManageLife.Commons;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UnassignPermissionsRequest
    {
        [Required]
        public string ObjectId { get; set; } = null!;
        public PermissionTargetType TargetType { get; set; }
        public List<string> PermissionIds { get; set; } = new List<string>();
    }
}
