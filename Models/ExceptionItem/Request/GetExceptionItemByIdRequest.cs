using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetExceptionItemByIdRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
