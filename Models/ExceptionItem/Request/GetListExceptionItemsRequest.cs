using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetListExceptionItemsRequest : IValidatableRequest
    {
        public string? Type { get; set; }
    }
}
