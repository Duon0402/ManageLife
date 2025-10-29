namespace ManageLife.Base
{
    public interface IRazorPageBase
    {
        RazorPageOptions Options { get; }

        void UseCss(params string[] cssUrls);

        void UseScriptAtBottom(params string[] jsUrls);

        void UseScriptAtHead(params string[] jsUrls);
    }
}
