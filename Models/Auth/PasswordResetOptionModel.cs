namespace ManageLife.Models
{
    /// <summary>
    /// Kênh đặt lại mật khẩu của tên đăng nhập: `telegram` = gửi mã qua Telegram đã liên kết;
    /// `none` = chưa liên kết (hoặc không có tài khoản) → phải liên hệ admin.
    /// </summary>
    public class PasswordResetOptionModel
    {
        public const string ChannelTelegram = "telegram";
        public const string ChannelNone = "none";

        public string Channel { get; set; } = ChannelNone;
    }
}
