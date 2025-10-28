namespace ManageLife.Base.View
{
    public class BreadcrumbModel
    {
        public BreadcrumbModel()
        {
            Items = new List<BreadcrumbItem>();
        }

        public List<BreadcrumbItem> Items { get; set; }
    }

    public class BreadcrumbItem
    {
        public BreadcrumbItem(string url, string title, bool isActive = false)
        {
            Url = url;
            Title = title;
            IsActive = isActive;
        }

        public string Url { get; set; } = null!;

        public string Title { get; set; } = null!;

        public bool IsActive { get; set; }
    }
}
