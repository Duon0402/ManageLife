using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace ManageLife.Helpers
{
    public static class FileUploaderHelper
    {
        public static IHtmlContent FileUploader(this IHtmlHelper htmlHelper, FileUploaderOptions options)
        {
            if (options.Id.IsEmpty())
                options.Id = IdHeper.NewId();

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

        #region HTML Render

        private static TagBuilder CreateContainer(FileUploaderOptions options)
        {
            var container = new TagBuilder("div");
            container.AddCssClass("file-uploader");
            container.Attributes["style"] = $"width: {options.Width};";

            if (options.CssClass.IsNotEmpty())
                container.AddCssClass(options.CssClass!);

            if (options.Id.IsNotEmpty())
                container.Attributes["id"] = options.Id;

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
            toolbar.Attributes["style"] = "display: none;";

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
                fileBrowseInput.Attributes.Add("multiple", "multiple");

            fileBrowseInput.Attributes.Add("accept", options.Accept);
            fileBrowseInput.Attributes.Add("hidden", "hidden");
            fileUploadBox.InnerHtml.AppendHtml(fileBrowseInput);

            return fileUploadBox;
        }

        #endregion

        #region Script Render

        private static TagBuilder CreateScript(FileUploaderOptions options)
        {
            var script = new TagBuilder("script");
            script.Attributes["type"] = "text/javascript";

            var id = options.Id ?? "fileUploader";
            var uploadUrl = options.UploadUrl;
            var maxFileCount = options.MaxFileCount ?? 0;
            var maxFileSize = options.MaxFileSize ?? 0;
            var uploadMode = options.UploadMode.ToString();
            var allowRemove = options.AllowRemove ? "true" : "false";
            var allowMultiFileUpload = options.AllowMultiFileUpload ? "true" : "false";

            var additionalDataJson = options.AdditionalData.IsNotEmpty()
                ? JsonSerializer.Serialize(options.AdditionalData)
                : "{}";

            var js = $@"
                (function(){{
                    {GenerateVariables(id)}
                    {GenerateUtils(maxFileCount, maxFileSize, allowRemove)}
                    {GenerateUploadFunction(uploadUrl, additionalDataJson)}
                    {GenerateHandleFiles(uploadMode, allowRemove)}
                    {GenerateEvents(uploadMode)}
                    {GenerateInit()}
                }})();
                ";

            script.InnerHtml.AppendHtml(js);
            return script;
        }

        private static string GenerateVariables(string id) => $@"
            var container = $('#{id}');
            var fileList = container.find('.file-list');
            var fileBrowseButton = container.find('.file-browse-button');
            var fileBrowseInput = container.find('.file-browse-input');
            var fileUploadBox = container.find('.file-upload-box');
            var statusText = container.find('.file-completed-status');
            var toolbar = container.find('.uploader-toolbar');

            var completedCount = 0;
            var totalCount = 0;
            var fileQueue = [];
        ";

        private static string GenerateUtils(int maxFileCount, long maxFileSize, string allowRemove) => $@"
            function updateStatus(){{
                statusText.text(completedCount + ' / ' + totalCount + ' Files Completed');
            }}

            function createFileItemHtml(file){{
                var ext = file.name.split('.').pop();
                var sizeMb = (file.size / (1024*1024)).toFixed(2);
                return `
                    <li class='file-item'>
                        <div class='file-extension'>${{ext}}</div>
                        <div class='file-content-wrapper'>
                            <div class='file-content'>
                                <div class='file-details'>
                                    <h5 class='file-name'>${{file.name}}</h5>
                                    <div class='file-info'>
                                        <small class='file-size'>${{sizeMb}} MB</small>
                                        <small class='file-divider'>-</small>
                                        <small class='file-status'>Pending</small>
                                    </div>
                                </div>
                                {(allowRemove == "true" ? "<button class='cancel-button'><i class='fa-solid fa-xmark'></i></button>" : "")}
                            </div>
                            <div class='file-progress-bar'>
                                <div class='file-progress'></div>
                            </div>
                        </div>
                    </li>
                `;
            }}

            function validateFile(file){{
                if ({maxFileCount} > 0 && (totalCount + 1) > {maxFileCount}) {{
                    alert('Max file count is {maxFileCount}');
                    return false;
                }}
                if ({maxFileSize} > 0 && file.size > {maxFileSize}) {{
                    alert('File ' + file.name + ' exceeds max size');
                    return false;
                }}
                return true;
            }}
        ";

        private static string GenerateUploadFunction(string uploadUrl, string additionalDataJson) => $@"
            function uploadFile(file, itemEl){{
                var formData = new FormData();
                formData.append('file', file);

                var extraData = {additionalDataJson};
                for (var key in extraData){{
                    formData.append(key, extraData[key]);
                }}

                ajaxService.upload('{uploadUrl}', formData, {{
                    showLoading: false,
                    hideLoading: false,
                    onProgress: function(percent){{
                        itemEl.find('.file-progress').css('width', percent + '%');
                        itemEl.find('.file-status').text('Uploading... ' + percent + '%');
                    }},
                    onSuccess: function(res){{
                        completedCount++;
                        updateStatus();
                        itemEl.find('.file-status').text('Completed');
                        itemEl.find('.file-progress').css('width', '100%');
                    }},
                    onError: function(err){{
                        completedCount++;
                        updateStatus();
                        itemEl.find('.file-status').text('Failed');
                        itemEl.find('.file-progress').css('background', 'red');
                    }}
                }});
            }}
        ";

        private static string GenerateHandleFiles(string uploadMode, string allowRemove) => $@"
            function handleSelectedFiles(files){{
                if (!files || files.length === 0) return;
                [...files].forEach(function(file){{
                    if (!validateFile(file)) return;

                    totalCount++;
                    updateStatus();

                    var $item = $(createFileItemHtml(file));
                    fileList.append($item);

                    if ('{uploadMode}' === 'Instant'){{
                        uploadFile(file, $item);
                    }} else {{
                        fileQueue.push({{file:file, el:$item}});
                        toolbar.show();
                    }}

                    if ({allowRemove}) {{
                        $item.find('.cancel-button').on('click', function(){{
                            $item.remove();
                            totalCount--;
                            updateStatus();
                        }});
                    }}
                }});
            }}
        ";

        private static string GenerateEvents(string uploadMode) => $@"
            fileUploadBox.on('drop', function(e){{
                e.preventDefault();
                handleSelectedFiles(e.originalEvent.dataTransfer.files);
                fileUploadBox.removeClass('active');
            }}).on('dragover', function(e){{
                e.preventDefault();
                fileUploadBox.addClass('active');
            }}).on('dragleave', function(e){{
                e.preventDefault();
                fileUploadBox.removeClass('active');
            }});

            fileBrowseButton.on('click', function(){{
                fileBrowseInput.click();
            }});

            fileBrowseInput.on('change', function(e){{
                handleSelectedFiles(e.target.files);
                fileBrowseInput.val('');
            }});

            container.find('.upload-all-button').on('click', function(){{
                fileQueue.forEach(function(item){{
                    uploadFile(item.file, item.el);
                }});
                fileQueue = [];
                toolbar.hide();
            }});

            container.find('.remove-all-button').on('click', function(){{
                fileList.empty();
                totalCount = 0;
                completedCount = 0;
                updateStatus();
                fileQueue = [];
                toolbar.hide();
            }});
        ";

        private static string GenerateInit() => "updateStatus();";

        #endregion
    }
}
