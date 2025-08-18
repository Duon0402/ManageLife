namespace ManageLife.Base
{
    public class MenuItem
    {
        public MenuItem(string title, string? url = null, string? icon = null, List<MenuItem>? subItems = null)
        {
            Title = title;
            Url = url;
            Icon = icon;
            SubItems = subItems ?? new List<MenuItem>();
        }

        public string Title { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public List<MenuItem> SubItems { get; set; }

        public bool HasChildren => SubItems.Any();
    }
}
