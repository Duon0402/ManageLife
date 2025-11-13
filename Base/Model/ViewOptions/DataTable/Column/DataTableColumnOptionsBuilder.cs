namespace ManageLife.Base
{
    public class DataTableColumnOptionsBuilder
    {
        private readonly DataTableColumnOptions _columnOptions = new();

        public DataTableColumnOptionsBuilder AriaTitle(string ariaTitle)
        {
            _columnOptions.AriaTitle = ariaTitle;
            return this;
        }

        public DataTableColumnOptionsBuilder CellType(DataTableColumnCellType cellType)
        {
            _columnOptions.CellType = cellType;
            return this;
        }

        public DataTableColumnOptionsBuilder ClassName(string className)
        {
            _columnOptions.ClassName = className;
            return this;
        }

        public DataTableColumnOptionsBuilder ContentPadding(string contentPadding)
        {
            _columnOptions.ContentPadding = contentPadding;
            return this;
        }

        public DataTableColumnOptionsBuilder CreatedCell(string createdCell)
        {
            _columnOptions.CreatedCell = createdCell;
            return this;
        }

        public DataTableColumnOptionsBuilder Data(DataTableColumnData data)
        {
            _columnOptions.Data = data;
            return this;
        }

        public DataTableColumnOptionsBuilder DefaultContent(string defaultContent)
        {
            _columnOptions.DefaultContent = defaultContent;
            return this;
        }

        public DataTableColumnOptionsBuilder Footer(string footer)
        {
            _columnOptions.Footer = footer;
            return this;
        }

        public DataTableColumnOptionsBuilder Name(string name)
        {
            _columnOptions.Name = name;
            return this;
        }

        public DataTableColumnOptionsBuilder Orderable(bool orderable)
        {
            _columnOptions.Orderable = orderable;
            return this;
        }

        public DataTableColumnOptionsBuilder OrderData(DataTableColumnOrderData orderData)
        {
            _columnOptions.OrderData = orderData;
            return this;
        }

        public DataTableColumnOptions Build()
        {
            return _columnOptions;
        }

        public static implicit operator DataTableColumnOptions(DataTableColumnOptionsBuilder builder)
        {
            return builder.Build();
        }
    }
}
