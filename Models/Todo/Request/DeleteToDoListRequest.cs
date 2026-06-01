using ManageLife.Core;
using System.ComponentModel.DataAnnotations;

namespace ManageLife.Models
{
    public class DeleteToDoListRequest : IValidatableRequest
    {
        [Required]
        public string Id { get; set; } = null!;
    }
}
