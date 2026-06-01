using ManageLife.Core;

namespace ManageLife.Models
{
    public class DeleteTodoTaskRequest : IValidatableRequest
    {
        public string Id { get; set; } = null!;
    }
}
