namespace ManageLife.Base
{
    public class DataTableColumnOptionsBuilder
    {
        private readonly DataTableColumnOptions _column = new();

        public DataTableColumnOptions Build()
        {
            return _column;
        }

        public DataTableColumnOptionsBuilder AriaTitle(string ariaTitle)
        {
            _column.AriaTitle = ariaTitle;
            return this;
        }

        public DataTableColumnOptionsBuilder CellType(DataTableColumnCellType cellType)
        {
            _column.CellType = cellType;
            return this;
        }

        public DataTableColumnOptionsBuilder ClassName(string className)
        {
            _column.ClassName = className;
            return this;
        }

        public DataTableColumnOptionsBuilder ContentPadding(string contentPadding)
        {
            _column.ContentPadding = contentPadding;
            return this;
        }

        public DataTableColumnOptionsBuilder CreatedCell(string createdCell)
        {
            _column.CreatedCell = createdCell;
            return this;
        }

        public DataTableColumnOptionsBuilder Data(DataTableColumnData data)
        {
            _column.Data = data;
            return this;
        }

        public DataTableColumnOptionsBuilder DefaultContent(string defaultContent)
        {
            _column.DefaultContent = defaultContent;
            return this;
        }

        public DataTableColumnOptionsBuilder Footer(string footer)
        {
            _column.Footer = footer;
            return this;
        }

        public DataTableColumnOptionsBuilder Name(string name)
        {
            _column.Name = name;
            return this;
        }

        public DataTableColumnOptionsBuilder Orderable(bool orderable)
        {
            _column.Orderable = orderable;
            return this;
        }

        public DataTableColumnOptionsBuilder OrderData(DataTableColumnOrderData orderData)
        {
            _column.OrderData = orderData;
            return this;
        }
    }
}
