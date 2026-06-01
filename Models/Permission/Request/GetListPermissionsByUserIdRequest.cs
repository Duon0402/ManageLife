using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetAssignedPermissionsByUserIdRequest : IValidatableRequest
    {
        [Required]
        public string UserId { get; set; } = null!;
    }
}
