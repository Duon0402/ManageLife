using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUnassignedPermissionsByRoleIdRequest
    {
        [Required]
        public string RoleId { get; set; } = null!;
    }
}
