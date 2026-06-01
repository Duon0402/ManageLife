using ManageLife.Core;

namespace ManageLife.Models
{
    public class RefreshTokenRequest : IValidatableRequest
    {
        public string RefreshToken { get; set; } = null!;
    }
}
