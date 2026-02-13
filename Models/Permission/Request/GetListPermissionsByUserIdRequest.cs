using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetAssignedPermissionsByUserIdRequest
    {
        [Required]
        public string UserId { get; set; } = null!;
    }
}
