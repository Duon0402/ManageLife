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

        public DataTableColumnOptionsBuilder CellType(DataTableCellType cellType)
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


        public DataTableColumnOptionsBuilder Data(string data)
        {
            _options.Data = data;
            return this;
        }

        public DataTableColumnOptionsBuilder Data(int data)
        {
            _options.Data = data;
            return this;
        }

        public DataTableColumnOptionsBuilder Data()
        {
            _options.Data = null;
            return this;
        }

        public DataTableColumnOptionsBuilder Data(object data)
        {
            _options.Data = data;
            return this;
        }

        public DataTableColumnOptionsBuilder Data(DataTableColumnData data)
        {
            _options.Data = data;
            return this;
        }
    }

    public delegate string DataTableColumnData(object row, string type, object set, object meta);
}
