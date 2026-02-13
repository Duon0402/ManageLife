using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetAssignedPermissionsByRoleIdRequest
    {
        [Required]
        public string RoleId { get; set; } = null!;
    }
}
