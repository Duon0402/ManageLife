using ManageLife.Base;

namespace ManageLife.Entities
{
    public class UserRefreshTokenEntity : EntityBase
    {
        public string UserId { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiryTime { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}