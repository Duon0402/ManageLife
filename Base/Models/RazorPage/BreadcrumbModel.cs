namespace ManageLife.Base
{
    public class BreadcrumbModel
    {
        public BreadcrumbModel()
        {
            Items = new List<BreadcrumbItem>();
        }

        public List<BreadcrumbItem> Items { get; set; }

        public void Add(string url, string title, bool isActive = false)
        {
            Items.Add(new(url, title, isActive));
        }

        public void Add(BreadcrumbItem item)
        {
            Items.Add(item);
        }

        public void AddRange(List<BreadcrumbItem> items)
        {
            Items.AddRange(items);
        }
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
