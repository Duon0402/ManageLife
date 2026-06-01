using ManageLife.Core;

namespace ManageLife.Models
{
    public class GetSettingByIdRequest : IValidatableRequest
    {
        public string Id { get; set; } = null!;
    }
}
