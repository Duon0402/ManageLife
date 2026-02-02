using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetListRolesByUserIdRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = null!;
    }
}
