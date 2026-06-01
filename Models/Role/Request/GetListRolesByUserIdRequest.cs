using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetListRolesByUserIdRequest : IValidatableRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = null!;
    }
}
