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

        protected virtual RazorPageOptions GetRazorPageOptions()
        {
            var options = ViewContext.ViewBag.PageOptions as RazorPageOptions;

            return options;
        }
    }
}
