using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetAssignedPermissionsByRoleIdRequest : IValidatableRequest
    {
        [Required]
        public string RoleId { get; set; } = null!;
    }
}
