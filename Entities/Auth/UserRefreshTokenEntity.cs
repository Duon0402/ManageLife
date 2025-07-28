using ManageLife.Base;
using ManageLife.Entities;

public class UserRefreshTokenEntity : IEntityBase
{
    public string Id { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiryTime { get; set; }
    public bool IsRevoked { get; set; } = false;

    public virtual UserEntity User { get; set; } = null!;
}
