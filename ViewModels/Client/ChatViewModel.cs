namespace ManageLife.ViewModels
{
    public class ChatViewModel
    {
        public string CurrentUserId { get; set; } = string.Empty;
        public List<ChatUserItem> Users { get; set; } = [];
    }

    public class ChatUserItem
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }
}
