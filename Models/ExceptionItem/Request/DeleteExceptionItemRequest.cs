using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteExceptionItemRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
