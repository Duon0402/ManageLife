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
        }

        public string Title { get; set; }
        public bool ShowTitle { get; set; }
        public BreadcrumbModel Breadcrumb { get; set; }
        public bool ShowBreadcrumb { get; set; }
        public List<string> ScriptBottomLinks { get; }
        public List<string> CssLinks { get; }
        public List<string> ScriptHeadLinks { get; }

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

        public void UseCss(params string[] cssUrls)
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

        public void UseScriptAtHead(params string[] jsUrls)
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

        public void UseScriptAtBottom(params string[] jsUrls)
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
