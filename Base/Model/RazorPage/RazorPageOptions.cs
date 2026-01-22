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
            ScriptBottomLinks = new();
            ScriptHeadLinks = new();
            CssLinks = new();
            UseFormData = false;
        }

        public string Title { get; set; }
        public bool ShowTitle { get; set; }
        public BreadcrumbModel Breadcrumb { get; set; }
        public bool ShowBreadcrumb { get; set; }
        public bool UseFormData { get; set; }
        public List<ResourceLink> ScriptBottomLinks { get; }
        public List<ResourceLink> CssLinks { get; }
        public List<ResourceLink> ScriptHeadLinks { get; }
        public string? BackUrl { get; private set; }

        public bool ShowBackButton => BackUrl.IsNotEmpty();

        public void EnableBackButton(string backUrl)
        {
            if (backUrl.IsNotEmpty())
            {
                throw new ArgumentException("BackUrl cannot be null or empty.", nameof(backUrl));
            }

            BackUrl = backUrl;
        }

        public void DisableBackButton()
        {
            BackUrl = null;
        }

        public bool HasHeadScripts
        {
            get
            {
                return ScriptHeadLinks.IsNotEmpty();
            }
        }

        public bool HasBottomScripts
        {
            get
            {
                return ScriptBottomLinks.IsNotEmpty();
            }
        }

        public bool HasCss
        {
            get
            {
                return CssLinks.IsNotEmpty();
            }
        }

        public void UseCss(params ResourceLink[] cssUrls)
        {
            if (cssUrls.IsEmpty())
            {
                return;
            }

            var lst = cssUrls.ToList();
            lst.Reverse();
            foreach (var item in lst)
            {
                if (!CssLinks.Contains(item))
                {
                    CssLinks.Insert(0, item);
                }
            }
        }

        public void UseScriptAtHead(params ResourceLink[] jsUrls)
        {
            if (jsUrls.IsEmpty())
            {
                return;
            }

            var lst = jsUrls.ToList();
            lst.Reverse();
            foreach (var item in lst)
            {
                if (!ScriptHeadLinks.Contains(item))
                {
                    ScriptHeadLinks.Insert(0, item);
                }
            }
        }

        public void UseScriptAtBottom(params ResourceLink[] jsUrls)
        {
            if (jsUrls.IsEmpty())
            {
                return;
            }

            var lst = jsUrls.ToList();
            lst.Reverse();
            foreach (var item in lst)
            {
                if (!ScriptBottomLinks.Contains(item))
                {
                    ScriptBottomLinks.Insert(0, item);
                }
            }
        }
    }
}
