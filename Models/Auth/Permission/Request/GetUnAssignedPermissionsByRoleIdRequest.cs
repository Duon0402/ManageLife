using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUnAssignedPermissionsByRoleIdRequest
    {
        [Required]
        public string RoleId { get; set; } = null!;
    }
}
