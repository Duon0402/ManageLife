using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace ManageLife.Helpers
{
    public static class DataGridHelper
    {
        public static IHtmlContent DataGrid<T>(this IHtmlHelper htmlHelper, DataGridViewOptions<T> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var gridBuilder = new TagBuilder("table");

            var id = !string.IsNullOrEmpty(options.Id)
                ? options.Id
                : $"datagrid_{Guid.NewGuid():N}";

            gridBuilder.Attributes["id"] = id;
            gridBuilder.AddCssClass("table table-bordered table-striped");

            if (options.CssClass.IsNotEmpty())
            {
                gridBuilder.AddCssClass(options.CssClass);
            }

            return gridBuilder;
        }

        private static TagBuilder CreateScript<T>(DataGridViewOptions<T> options)
        {
            var tableId = options.Id;

            var script = new TagBuilder("script");
            script.Attributes["type"] = "text/javascript";

            var sb = new StringBuilder();
            sb.AppendLine("");

            return script;
        }
    }
}
