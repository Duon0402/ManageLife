using ManageLife.Contexts;
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

        protected ITranslationContext TranslationContext =>
            ViewContext?.HttpContext?.RequestServices
                .GetRequiredService<ITranslationContext>()
            ?? throw new InvalidOperationException("TranslationContext is not available.");

        public string T(string key, params object[] args)
        {
            return TranslationContext.TranslateAsync(key, args).GetAwaiter().GetResult();
        }

        public string T(string key, string languageCode, params object[] args)
        {
            return TranslationContext.TranslateAsync(key, languageCode, args).GetAwaiter().GetResult();
        }

        public void UseCss(params ResourceLink[] cssUrls)
        {
            Options.UseCss(cssUrls);
        }

        public void UseScriptAtBottom(params ResourceLink[] jsUrls)
        {
            Options.UseScriptAtBottom(jsUrls);
        }

        public void UseScriptAtHead(params ResourceLink[] jsUrls)
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
