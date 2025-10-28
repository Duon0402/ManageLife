using ManageLife.Base.View;

namespace ManageLife.Base
{
    public class RazorPageOption
    {
        public RazorPageOption()
        {
           
        }

        public string? Title { get; set; }
        public bool ShowTitle { get; set; }


        public BreadcrumbModel Breadcrumb { get; set; }
        public bool ShowBreadcrumb { get; set; }

    }
}
