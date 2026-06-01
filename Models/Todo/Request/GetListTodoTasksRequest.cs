using ManageLife.Commons;
using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetListTodoTasksRequest : IValidatableRequest
    {
        public string? TodoListId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TodoStatus? Status { get; set; }
        public TodoPriority? Priority { get; set; }
    }
}
