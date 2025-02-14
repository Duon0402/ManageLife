using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
    public static class PopupHelper
    {
        public static IHtmlContent Popup(this IHtmlHelper htmlHelper, PopupOptions options)
        {
            var modalContainer = new TagBuilder("div");
            modalContainer.AddCssClass("modal fade");
            modalContainer.Attributes.Add("id", options.Id);
            modalContainer.Attributes.Add("tabindex", "-1");
            modalContainer.Attributes.Add("aria-hidden", "true");

            var modalDialog = new TagBuilder("div");
            modalDialog.AddCssClass("modal-dialog");
            if (!string.IsNullOrWhiteSpace(options.Width))
            {
                modalDialog.Attributes.Add("style", $"width: {options.Width};");
            }
            if (!string.IsNullOrWhiteSpace(options.MinWidth))
            {
                var currentStyle = modalDialog.Attributes.ContainsKey("style") ? modalDialog.Attributes["style"] : "";
                modalDialog.Attributes["style"] = currentStyle + $" min-width: {options.MinWidth};";
            }

            var modalContent = new TagBuilder("div");
            modalContent.AddCssClass("modal-content");

            if (options.ShowTitle)
            {
                var modalHeader = new TagBuilder("div");
                modalHeader.AddCssClass("modal-header");

                var titleTag = new TagBuilder("h5");
                titleTag.AddCssClass("modal-title");
                titleTag.InnerHtml.Append(options.Title);
                modalHeader.InnerHtml.AppendHtml(titleTag);

                if (options.ShowCloseButton)
                {
                    var closeButton = new TagBuilder("button");
                    closeButton.AddCssClass("btn-close");
                    closeButton.Attributes.Add("data-bs-dismiss", "modal");
                    closeButton.Attributes.Add("aria-label", "Close");
                    modalHeader.InnerHtml.AppendHtml(closeButton);
                }

                modalContent.InnerHtml.AppendHtml(modalHeader);
            }

            var modalBody = new TagBuilder("div");
            modalBody.AddCssClass("modal-body");
            if (options.Content != null)
            {
                modalBody.InnerHtml.AppendHtml(options.Content);
            }
            modalContent.InnerHtml.AppendHtml(modalBody);

            modalDialog.InnerHtml.AppendHtml(modalContent);
            modalContainer.InnerHtml.AppendHtml(modalDialog);

            if (!string.IsNullOrWhiteSpace(options.Height))
            {
                modalContent.Attributes.Add("style", $"height: {options.Height};");
            }
            if (!string.IsNullOrWhiteSpace(options.MinHeight))
            {
                var currentStyle = modalContent.Attributes.ContainsKey("style") ? modalContent.Attributes["style"] : "";
                modalContent.Attributes["style"] = currentStyle + $" min-height: {options.MinHeight};";
            }

            return modalContainer;
        }
    }
}
