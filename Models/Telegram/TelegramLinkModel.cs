namespace ManageLife.Models
{
    /// <summary>Liên kết mở bot Telegram kèm mã dùng 1 lần (t.me/&lt;bot&gt;?start=&lt;mã&gt;).</summary>
    public class TelegramLinkModel
    {
        public string DeepLink { get; set; } = null!;
        /// <summary>UTC.</summary>
        public DateTime ExpiresAt { get; set; }
    }
}
