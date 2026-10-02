namespace ManageLife.Models
{
    /// <summary>Thông tin tài khoản của chính người dùng đang đăng nhập (không chứa dữ liệu nhạy cảm).</summary>
    public class AccountModel
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool TelegramLinked { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
