using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetUserByIdRequest
    {
        [Required(ErrorMessage = ("UserId is required"))]
        public string UserId { get; set; } = null!;
    }
}
