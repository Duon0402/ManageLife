namespace ManageLife.Base
{
    public class DataTableOptions
    {
        /// <summary>
        /// Data to use as the display data for the table.
        /// </summary>
        public List<object>? Data { get; set; }

        /// <summary>
        /// Load data for the table's content from an Ajax source
        /// </summary>
        public DataTableAjaxOptions? Ajax { get; set; }

        /// <summary>
        /// Set column specific initialisation properties.
        /// </summary>
        public List<DataTableColumnOptions>? Columns { get; set; } = new();

        /// <summary>
        /// Set column definition initialisation properties.
        /// </summary>
        public List<DataTableColumnDefsOptions>? ColumnDefs { get; set; } = new();
    }
}
