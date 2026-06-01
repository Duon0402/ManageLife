using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetRoleByIdRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "RoleId is required")]
        public string RoleId { get; set; } = null!;
    }
}
