using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
    public static class FileUploaderHelper
    {
        #region Render HTML
        public static IHtmlContent FileUploader(this IHtmlHelper htmlHelper, FileUploaderOptions options)
        {
            if (options.Id.IsEmpty())
            {
                options.Id = IdHeper.NewId();
            }

            var container = CreateContainer(options);
            container.InnerHtml.AppendHtml(CreateHeader(options));
            container.InnerHtml.AppendHtml(CreateToolbar(options));
            container.InnerHtml.AppendHtml(CreateFileList());
            container.InnerHtml.AppendHtml(CreateUploadBox(options));

            var wrapper = new HtmlContentBuilder();
            wrapper.AppendHtml(container);
            wrapper.AppendHtml(CreateScript(options));

            return wrapper;
        }

        private static TagBuilder CreateContainer(FileUploaderOptions options)
        {
            var container = new TagBuilder("div");
            container.AddCssClass("file-uploader");
            container.Attributes["style"] = $"width: {options.Width};";

            if (options.CssClass.IsNotEmpty())
            {
                container.AddCssClass(options.CssClass!);
            }

            if (options.Id.IsNotEmpty())
            {
                container.Attributes["id"] = options.Id;
            }

            return container;
        }

        private static TagBuilder CreateHeader(FileUploaderOptions options)
        {
            var header = new TagBuilder("div");
            header.AddCssClass("uploader-header");

            var title = new TagBuilder("h2");
            title.AddCssClass("uploader-title");
            title.InnerHtml.Append(options.Title);
            header.InnerHtml.AppendHtml(title);

            var fileCompletedStatus = new TagBuilder("h4");
            fileCompletedStatus.AddCssClass("file-completed-status");
            header.InnerHtml.AppendHtml(fileCompletedStatus);

            return header;
        }

        private static TagBuilder CreateToolbar(FileUploaderOptions options)
        {
            var toolbar = new TagBuilder("div");
            toolbar.AddCssClass("uploader-toolbar");
            toolbar.Attributes["style"] = $"display: none;";

            if (options.UploadMode == UploadMode.OnButtonClick)
            {
                var uploadAllButton = new TagBuilder("button");
                uploadAllButton.AddCssClass("upload-all-button");
                uploadAllButton.InnerHtml.AppendHtml("<i class=\"fa-solid fa-arrow-up-from-bracket\"></i>");
                uploadAllButton.InnerHtml.AppendHtml("<span class=\"ms-1\">Upload all</span>");
                toolbar.InnerHtml.AppendHtml(uploadAllButton);
            }

            if (options.AllowRemove)
            {
                var removeAllButton = new TagBuilder("button");
                removeAllButton.AddCssClass("remove-all-button");
                removeAllButton.InnerHtml.AppendHtml("<i class=\"fa-solid fa-trash\"></i>");
                removeAllButton.InnerHtml.AppendHtml("<span class=\"ms-1\">Remove all</span>");
                toolbar.InnerHtml.AppendHtml(removeAllButton);
            }

            return toolbar;
        }

        private static TagBuilder CreateFileList()
        {
            var fileList = new TagBuilder("ul");
            fileList.AddCssClass("file-list");
            return fileList;
        }

        private static TagBuilder CreateUploadBox(FileUploaderOptions options)
        {
            var fileUploadBox = new TagBuilder("div");
            fileUploadBox.AddCssClass("file-upload-box");

            var boxTitle = new TagBuilder("h2");
            boxTitle.AddCssClass("box-title");

            var fileInstruction = new TagBuilder("span");
            fileInstruction.AddCssClass("file-instruction");
            fileInstruction.InnerHtml.AppendHtml("Drag files here or ");
            boxTitle.InnerHtml.AppendHtml(fileInstruction);

            var fileBrowseButton = new TagBuilder("span");
            fileBrowseButton.AddCssClass("file-browse-button");
            fileBrowseButton.InnerHtml.AppendHtml("browse");
            boxTitle.InnerHtml.AppendHtml(fileBrowseButton);

            fileUploadBox.InnerHtml.AppendHtml(boxTitle);

            var fileBrowseInput = new TagBuilder("input");
            fileBrowseInput.AddCssClass("file-browse-input");
            fileBrowseInput.Attributes.Add("type", "file");

            if (options.AllowMultiFileUpload)
            {
                fileBrowseInput.Attributes.Add("multiple", "multiple");
            }

            fileBrowseInput.Attributes.Add("accept", options.Accept);
            fileBrowseInput.Attributes.Add("hidden", "hidden");
            fileUploadBox.InnerHtml.AppendHtml(fileBrowseInput);

            return fileUploadBox;
        }
        #endregion

        #region Render JS
        private static TagBuilder CreateScript(FileUploaderOptions options)
        {
            var script = new TagBuilder("script");
            script.Attributes["type"] = "text/javascript";

            var js = "";

            script.InnerHtml.AppendHtml(js);
            return script;
        }


        private static string GenerateVariables(FileUploaderOptions options)
        {
            var id = options.Id ?? "fileUploader";

            return $@"
                var container = $('#{id}');
                var fileList = container.find('.file-list');
                var fileBrowseButton = container.find('.file-browse-button');
                var fileBrowseInput = container.find('.file-browse-input');
                var fileUploadBox = container.find('.file-upload-box');
            ";
        }

        private static string GenerateDragDropScript(FileUploaderOptions options)
        {
            var id = options.Id ?? "fileUploader";

            return string.Empty;
        }
        #endregion
    }
}