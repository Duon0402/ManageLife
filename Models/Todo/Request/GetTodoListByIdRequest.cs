using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class GetTodoListByIdRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
