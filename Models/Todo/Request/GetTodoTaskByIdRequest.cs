using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetTodoTaskByIdRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
