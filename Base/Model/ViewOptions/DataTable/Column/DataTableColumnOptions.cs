using System.Text.Json.Serialization;

namespace ManageLife.Base
{
    public class DataTableColumnOptions
    {
        /// <summary>
        /// Set the columns' aria-label attribute value
        /// </summary>
        public string? AriaTitle { get; set; }

        /// <summary>
        /// Cell type to be created for a column
        /// </summary>
        [JsonConverter(typeof(DataTableCellTypeConverter))]
        public DataTableCellType? CellType { get; set; }

        /// <summary>
        /// Class to assign to each cell in the column
        /// </summary>
        public string? ClassName { get; set; }

        /// <summary>
        /// Add padding to the text content used when calculating the optimal width for a table.
        /// </summary>
        public string? ContentPadding { get; set; }

        /// <summary>
        /// Cell created callback to allow DOM manipulation
        /// Parameters: 
        /// td - cell node, 
        /// cellData - original cell data, 
        /// rowData - full row data, 
        /// rowIndex - internal row index, 
        /// colIndex - internal column index.
        /// Example: 
        /// "function(td, cellData, rowData, rowIndex, colIndex) { ... }"
        /// </summary>
        public string? CreatedCell { get; set; }

        /// <summary>
        /// Set the data source for the column from the rows data object / array
        /// Type: integer, string, null, object, data(row: array/object, type: string, set: any, meta: object)
        /// Link: "https://datatables.net/reference/option/columns.data"
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// Set default, static, content for a column
        /// </summary>
        public string? DefaultContent { get; set; }

        /// <summary>
        /// Set the column footer text
        /// </summary>
        public string? Footer { get; set; }

        /// <summary>
        /// Set a descriptive name for a column
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Enable or disable ordering on this column
        /// </summary>
        public bool? Orderable { get; set; }

        /// <summary>
        /// Define multiple column ordering as the default order for a column
        /// Type: interger, array
        /// </summary>
        public object? OrderData { get; set; }

        /// <summary>
        /// Live DOM sorting type assignment
        /// </summary>
        public string? OrderDataType { get; set; }

        [JsonConverter(typeof(DataTableOrderDirectionConverter))]
        /// <summary>
        /// Order direction application sequence
        /// </summary>
        public DataTableOrderDirection[]? OrderSequence { get; set; }

        /// <summary>
        /// Render (process) the data for use in the table
        /// Type: integer, string, object, array, render(data: any, type: string, row: any, meta: object)
        /// Link: "https://datatables.net/reference/option/columns.render"
        /// </summary>
        public object? Render { get; set; }

        /// <summary>
        /// Enable or disable search on the data in this column
        /// </summary>
        public bool? Searchable { get; set; }

        /// <summary>
        /// Set the column title
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Set the column type - used for filtering and sorting string processing
        /// </summary>
        [JsonConverter(typeof(DataTableColumnTypeConverter))]
        public DataTableColumnType? Type { get; set; }

        /// <summary>
        /// Enable or disable the display of this column
        /// </summary>
        public bool? Visible { get; set; }

        /// <summary>
        /// Column width assignment
        /// </summary>
        public string? Width { get; set; }
    }
}
