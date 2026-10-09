namespace ManageLife.Models
{
    /// <summary>Một phiên đăng nhập (thiết bị) của tài khoản.</summary>
    public class AccountSessionModel
    {
        public string SessionId { get; set; } = null!;
        public string? DeviceName { get; set; }
        /// <summary>UTC.</summary>
        public DateTime StartedAt { get; set; }
        /// <summary>UTC — lần làm mới phiên gần nhất.</summary>
        public DateTime LastActiveAt { get; set; }
        /// <summary>Phiên của chính thiết bị đang gọi.</summary>
        public bool IsCurrent { get; set; }
    }
}
