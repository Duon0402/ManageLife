using ManageLife.Base.Model.ViewOptions.DataTable.Column;

namespace ManageLife.Base
{
    public class DataTableColumnOptionsBuilder
    {
        private readonly DataTableColumnOptions _options = new();

        public DataTableColumnOptions Build()
        {
            return _options;
        }

        public DataTableColumnOptionsBuilder AriaTitle(string ariaTitle)
        {
            _options.AriaTitle = ariaTitle;
            return this;
        }

        public DataTableColumnOptionsBuilder CellType(DataTableColumnCellType cellType)
        {
            _options.CellType = cellType;
            return this;
        }

        public DataTableColumnOptionsBuilder ClassName(string className)
        {
            _options.ClassName = className;
            return this;
        }

        public DataTableColumnOptionsBuilder ContentPadding(string contentPadding)
        {
            _options.ContentPadding = contentPadding;
            return this;
        }

        public DataTableColumnOptionsBuilder CreatedCell(string createdCell)
        {
            _options.CreatedCell = createdCell;
            return this;
        }

        public DataTableColumnOptionsBuilder Data(DataTableColumnData data)
        {
            _options.Data = data;
            return this;
        }

        public DataTableColumnOptionsBuilder DefaultContent(string defaultContent)
        {
            _options.DefaultContent = defaultContent;
            return this;
        }

        public DataTableColumnOptionsBuilder Footer(string footer)
        {
            _options.Footer = footer;
            return this;
        }

        public DataTableColumnOptionsBuilder Name(string name)
        {
            _options.Name = name;
            return this;
        }

        public DataTableColumnOptionsBuilder Orderable(bool orderable)
        {
            _options.Orderable = orderable;
            return this;
        }

        public DataTableColumnOptionsBuilder OrderData(DataTableColumnOrderData orderData)
        {
            _options.OrderData = orderData;
            return this;
        }
    }
}
