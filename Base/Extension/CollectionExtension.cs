using ManageLife.Base.Model.ViewOptions.DataTable.Ajax;
using System.Diagnostics.CodeAnalysis;

namespace ManageLife.Base
{
    public static class CollectionExtension
    {
        public static bool IsEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? collection)
            => collection == null || !collection.Any();

        public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? collection)
            => collection != null && collection.Any();

        public static DataTableAjaxDataSrcResult ConvertDataTableResult<T>(
            this IEnumerable<T>? collection,
            int draw = 0,
            int? recordsFiltered = null,
            int? recordsTotal = null,
            string? error = null)
        {
            var list = collection?.ToList() ?? new List<T>();

            return new DataTableAjaxDataSrcResult
            {
                Draw = draw,
                RecordsTotal = recordsTotal ?? list.Count,
                RecordsFiltered = recordsFiltered ?? list.Count,
                Data = list.Cast<object>().ToList(),
                Error = error
            };
        }
    }
}
