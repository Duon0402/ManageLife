namespace ManageLife.Base
{
    public class MenuItem
    {
        // Nếu có subItems thì không cần url
        public MenuItem(string title, string icon, List<MenuItem> subItems)
        {
            Title = title;
            Url = string.Empty;
            Icon = icon;
            SubItems = subItems;
        }

        // Nếu không có subItems thì cần url
        public MenuItem(string title, string url, string icon)
        {
            Title = title;
            Url = url;
            Icon = icon;
            SubItems = new List<MenuItem>();
        }

        public string Title { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public List<MenuItem>? SubItems { get; set; }
    }
}
