using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteRoleRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "RoleId is required")]
        public string RoleId { get; set; } = null!;
    }
}
