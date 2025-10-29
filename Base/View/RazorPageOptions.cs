namespace ManageLife.Base
{
    public class RazorPageOptions
    {
        public RazorPageOptions()
        {
            Breadcrumb = new();
            ShowBreadcrumb = true;
            Title = string.Empty;
            ShowTitle = true;
        }

        public string? Title { get; set; }
        public bool ShowTitle { get; set; }

        public BreadcrumbModel Breadcrumb { get; set; }
        public bool ShowBreadcrumb { get; set; }
    }
}
