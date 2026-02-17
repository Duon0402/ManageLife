namespace ManageLife.Base
{
    public class DataTableColumnData
    {
        public object? Value { get; set; }

        public DataTableColumnData(object? value)
        {
            Value = value;
        }

        /// <summary>
        /// Use an integer as the column index (array index in the data source).
        /// Example: 0, 1, 2…
        /// </summary>
        public static implicit operator DataTableColumnData(int value) => new(value);

        /// <summary>
        /// Use a string as the column data property name.
        /// Special options:
        /// - "." → dotted notation for nested objects, e.g. browser.version
        /// - "[]" → array notation, e.g. name[, ]
        /// - "()" → function notation, e.g. browser()
        /// </summary>
        public static implicit operator DataTableColumnData(string value) => new(value);

        /// <summary>
        /// Use a delegate to generate a JS function for the column.
        /// Signature: (row, type, set, meta) => string
        /// Example: (row, type, set, meta) => "return row.Id + '-' + type;"
        /// </summary>
        public static implicit operator DataTableColumnData(Func<DataTableColumnDataDelegate, string> func) => new(func);
    }

    /// <summary>
    /// Delegate for type-safe column callback: row, type, set, meta → string
    /// </summary>
    public delegate string DataTableColumnDataDelegate(object row, string type, object? set, object meta);
}
