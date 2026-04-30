using Microsoft.AspNetCore.Mvc.Razor;

namespace ManageLife.Core
{
    public static class RazorPageExtension
    {
        public static void SetTitle(this RazorPage page, string title)
        {
            if (title.IsEmpty()) return;

            var options = page.GetRazorPageOptions();
            options.Title = title;
        }

        public static string GetTitle(this RazorPageBase page)
        {
            var title = page.GetRazorPageOptions().Title.Trim();
            return title;
        }

        public static RazorPageOptions GetRazorPageOptions(this RazorPageBase page)
        {
            var options = page.ViewBag.RazorPageOptions as RazorPageOptions;
            if (options == null)
            {
                options = new RazorPageOptions();
                page.ViewBag.RazorPageOptions = options;
            }

            return options;
        }
    }
}
