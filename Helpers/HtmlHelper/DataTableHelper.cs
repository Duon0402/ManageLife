using ManageLife.Base;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManageLife.Helpers
{
    public static class DataTableHelper
    {
        // TODO: Phải sửa lại DataTableHelper
        public static IHtmlContent DataTable<T>(this IHtmlHelper htmlHelper, DataTableColumnOptions options)
        {
            return null;
        }

        public static IHtmlContent DataTable(this IHtmlHelper htmlHelper, DataTableColumnOptions options)
        {
            return htmlHelper.DataTable<object>(options);
        }
    }
}
