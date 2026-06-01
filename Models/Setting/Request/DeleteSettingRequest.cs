using ManageLife.Core;

namespace ManageLife.Models
{
    public class DeleteSettingRequest : IValidatableRequest
    {
        public string? Key { get; set; }
        public string? Id { get; set; }
    }
}
