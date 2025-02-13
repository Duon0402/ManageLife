using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
    public static class HtmlHelper
    {
        // TODO: Thêm phần action để tái sử dụng
        // TODO: Thêm searching, filtering, và paging ...
        #region DataGrid
        public static IHtmlContent DataGrid<T>(this IHtmlHelper htmlHelper, IEnumerable<T> items, IEnumerable<string> columnNames, DataGridOptions? options = null)
        {
            var gridBuilder = new TagBuilder("div");
            gridBuilder.AddCssClass("data-grid-container");

            if (!string.IsNullOrWhiteSpace(options?.Id))
            {
                gridBuilder.Attributes.Add("id", options.Id);
            }

            #region Toolbar
            var toolBarBuilder = new TagBuilder("div");
            toolBarBuilder.AddCssClass("d-flex justify-content-end mb-3");

            // Insert Button
            if (options?.AllowInsert == true)
            {
                var insertButton = new TagBuilder("button");
                insertButton.AddCssClass("btn btn-primary me-2");
                insertButton.InnerHtml.Append("Insert");
                // TODO: Thêm hàm xử lý Insert có thể tái sử dụng
                insertButton.Attributes.Add("onclick", "insertFunction()");
                toolBarBuilder.InnerHtml.AppendHtml(insertButton);
            }

            gridBuilder.InnerHtml.AppendHtml(toolBarBuilder);
            #endregion

            #region Table
            var tableContainer = new TagBuilder("div");
            tableContainer.AddCssClass("table-responsive");

            var tableBuilder = new TagBuilder("table");
            tableBuilder.AddCssClass("table table-bordered table-hover");

            // Header
            var theadBuilder = new TagBuilder("thead");
            theadBuilder.AddCssClass("table-secondary");

            var headerRowBuilder = new TagBuilder("tr");
            foreach (var columnName in columnNames)
            {
                var thBuilder = new TagBuilder("th");
                thBuilder.InnerHtml.Append(columnName);
                headerRowBuilder.InnerHtml.AppendHtml(thBuilder);
            }
            theadBuilder.InnerHtml.AppendHtml(headerRowBuilder);
            tableBuilder.InnerHtml.AppendHtml(theadBuilder);

            // Body
            var tbodyBuilder = new TagBuilder("tbody");

            // TODO: Thêm lựa chọn đặt tên cho cột
            foreach (var item in items)
            {
                var rowBuilder = new TagBuilder("tr");

                foreach (var columnName in columnNames)
                {
                    var tdBuilder = new TagBuilder("td");
                    var property = typeof(T).GetProperty(columnName);
                    if (property != null)
                    {
                        var value = property.GetValue(item)?.ToString() ?? string.Empty;
                        tdBuilder.InnerHtml.Append(value);
                    }
                    rowBuilder.InnerHtml.AppendHtml(tdBuilder);
                }

                tbodyBuilder.InnerHtml.AppendHtml(rowBuilder);
            }

            tableBuilder.InnerHtml.AppendHtml(tbodyBuilder);

            tableContainer.InnerHtml.AppendHtml(tableBuilder);
            #endregion

            gridBuilder.InnerHtml.AppendHtml(tableContainer);

            return gridBuilder;
        }
        #endregion

        #region Form
        public static IHtmlContent Form<T>(this IHtmlHelper htmlHelper, FormOptions? options = null)
        {
            var formContainer = new TagBuilder("div");
            formContainer.AddCssClass("form-container");

            if (string.IsNullOrWhiteSpace(options?.Id))
            {
                formContainer.Attributes.Add("id", options?.Id);
            }

            var formBuilder = new TagBuilder("form");

            return formContainer;
        }
        #endregion

        #region TextBox
        public static IHtmlContent TextBox(this IHtmlHelper htmlHelper, TextBoxOptions options)
        {
            // TODO: Bổ sung phần lable
            var textBoxBuilder = new TagBuilder("div");

            // Lable
            var lableBuilder = new TagBuilder("lable");
            lableBuilder.Attributes.Add("for", options.Name);
            lableBuilder.Attributes.Add("value", options.Lable ?? options.Name);
            textBoxBuilder.InnerHtml.AppendHtml(lableBuilder);

            // Input
            var inputBuilder = new TagBuilder("input");
            inputBuilder.Attributes.Add("type", "text");
            inputBuilder.Attributes.Add("name", options.Name);

            if (string.IsNullOrWhiteSpace(options.Id))
            {
                inputBuilder.Attributes.Add("id", options.Id);
            }

            if (string.IsNullOrWhiteSpace(options.Value))
            {
                inputBuilder.Attributes.Add("value", options.Value);
            }

            if (string.IsNullOrWhiteSpace(options.CssClass))
            {
                inputBuilder.Attributes.Add("class", options.CssClass);
            }

            textBoxBuilder.InnerHtml.AppendHtml(inputBuilder);

            return textBoxBuilder;
        }
        #endregion
    }
}
