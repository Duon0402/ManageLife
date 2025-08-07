using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
    public static class FileUploaderHelper
    {
        public static IHtmlContent FileUploader(this IHtmlHelper htmlHelper, FileUploaderOptions options)
        {

            var fileUploaderBuilder = new TagBuilder("div");
            fileUploaderBuilder.AddCssClass("file-uploader");
            if (options.Id.IsNotEmpty())
            {
                fileUploaderBuilder.Attributes.Add("id", options.Id);
            }

            return fileUploaderBuilder;
        }
    }
}