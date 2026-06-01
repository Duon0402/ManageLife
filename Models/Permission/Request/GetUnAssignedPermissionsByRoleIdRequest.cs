using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUnassignedPermissionsByRoleIdRequest : IValidatableRequest
    {
        [Required]
        public string RoleId { get; set; } = null!;
    }
}
