using ManageLife.Core;

namespace ManageLife.Entities
{
    public class UserRefreshTokenEntity : EntityBase
    {
        public string UserId { get; set; } = default!;
        /// <summary>SHA-256 (base64) của refresh token — không lưu token gốc.</summary>
        public string RefreshToken { get; set; } = default!;
        public DateTime ExpiryTime { get; set; }
        public bool IsRevoked { get; set; } = false;
        /// <summary>UTC — lúc bị thu hồi (xoay vòng hoặc đăng xuất).</summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>Id phiên đăng nhập: tạo khi đăng nhập, giữ nguyên qua các lần làm mới token.</summary>
        public string SessionId { get; set; } = default!;
        public string? DeviceName { get; set; }
        /// <summary>UTC — lúc đăng nhập (bắt đầu phiên).</summary>
        public DateTime SessionStartedAt { get; set; }
        /// <summary>UTC — lúc cấp token này (≈ lần hoạt động gần nhất của phiên).</summary>
        public DateTime IssuedAt { get; set; }
    }
}
