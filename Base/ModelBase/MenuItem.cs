namespace ManageLife.Base
{
	public class MenuItem
	{
		public MenuItem(string title, string url, string icon, List<MenuItem> subItems)
		{
			Title = title;
			Url = url;
			Icon = icon;
			SubItems = subItems;
		}

        public MenuItem(string title, string url, string icon)
        {
            Title = title;
			Url = url;
			Icon = icon;
			SubItems = new List<MenuItem>();
		}

        public string Title { get; set; }
		public string Url { get; set; }
		public string? Icon { get; set; }
		public List<MenuItem>? SubItems { get; set; }
	}
}
