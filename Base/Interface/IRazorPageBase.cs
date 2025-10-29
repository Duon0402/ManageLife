using ManageLife.Base.Model;

namespace ManageLife.Base
{
    public interface IRazorPageBase
    {
        RazorPageOptions Options { get; }

        void UseCss(params ResourceLink[] cssUrls);

        void UseScriptAtBottom(params ResourceLink[] jsUrls);

        void UseScriptAtHead(params ResourceLink[] jsUrls);
    }
}
