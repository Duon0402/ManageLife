namespace ManageLife.Base
{
    public class DataTableOptions
    {
        public List<DataTableColumnOptions> Columns { get; set; } = new();

        /// <summary>
        /// Load data for the table's content from an Ajax source
        /// </summary>
        public object? Ajax { get; set; }
    }
}
