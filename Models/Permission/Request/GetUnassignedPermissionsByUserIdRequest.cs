using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUnassignedPermissionsByUserIdRequest : IValidatableRequest
    {
        [Required]
        public string UserId { get; set; } = null!;
    }
}
