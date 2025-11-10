namespace ManageLife.Base
{
    public class DataTableColumnDefsOptions : DataTableColumnOptions
    {
        /// <summary>
        /// Assign a column definition to one or more columns.
        /// Type: array, string, integer
        /// Link: "https://datatables.net/reference/option/columnDefs.targets"
        /// </summary>
        public object Targets { get; set; } = null!;
    }
}
