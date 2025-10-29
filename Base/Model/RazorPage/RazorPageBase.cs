using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Base
{
    public abstract class RazorPageBase<TModel> : RazorPage<TModel>, IRazorPageBase
    {
        public RazorPageOptions Options
        {
            get
            {
                return GetRazorPageOptions();
            }
        }

        public void UseCss(params string[] cssUrls)
        {
            Options.UseCss(cssUrls);
        }

        public void UseScriptAtBottom(params string[] jsUrls)
        {
            Options.UseScriptAtBottom(jsUrls);
        }

        public void UseScriptAtHead(params string[] jsUrls)
        {
            Options.UseScriptAtHead(jsUrls);
        }

        protected virtual RazorPageOptions GetRazorPageOptions()
        {
            var options = ViewContext.ViewBag.RazorPageOptions as RazorPageOptions;

            if (options == null)
            {
                options = new RazorPageOptions();
                ViewContext.ViewBag.RazorPageOptions = options;
            }

            return options;
        }
    }
}
