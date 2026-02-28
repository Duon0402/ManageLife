namespace ManageLife.Base
{
    public interface IRazorPageBase
    {
        RazorPageOptions Options { get; }

        void UseCss(params ResourceLink[] cssUrls);

        void UseScriptAtBottom(params ResourceLink[] jsUrls);

        void UseScriptAtHead(params ResourceLink[] jsUrls);

        string T(string key, params object[] args);
        string T(string key, string languageCode, params object[] args);
    }
}
