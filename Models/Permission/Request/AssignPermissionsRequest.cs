using ManageLife.Commons;
using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class AssignPermissionsRequest : IValidatableRequest
    {
        [Required]
        public string ObjectId { get; set; } = null!;
        public PermissionTargetType TargetType { get; set; }
        public List<string> PermissionIds { get; set; } = new List<string>();
    }
}
