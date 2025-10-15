using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetTodoListByIdRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
