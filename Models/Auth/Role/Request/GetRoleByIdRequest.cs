using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetRoleByIdRequest
    {
        [Required(ErrorMessage = "RoleId is required")]
        public string RoleId { get; set; } = null!;
    }
}
