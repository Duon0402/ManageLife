using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteToDoListRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
