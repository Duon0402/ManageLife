using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class UpdateTodoTaskRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
