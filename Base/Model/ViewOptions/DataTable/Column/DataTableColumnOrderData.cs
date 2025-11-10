namespace ManageLife.Base
{
    /// <summary>
    /// Define multiple column ordering as the default order for a column.
    /// </summary>
    public class DataTableColumnOrderData
    {
        public object? Value { get; set; }

        public DataTableColumnOrderData(object? value)
        {
            Value = value;
        }

        /// <summary>
        /// A single column index to order upon
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator DataTableColumnOrderData(int value) => new(value);

        /// <summary>
        /// Multiple column indexes to define multi-column sorting
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator DataTableColumnOrderData(List<int> value) => new(value);
    }
}
