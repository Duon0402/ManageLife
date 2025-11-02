namespace ManageLife.Base
{
    public class DataGridColumn
    {
        public DataGridColumn() { }

        public DataGridColumn(string fieldName, string? headerText = null)
        {
            FieldName = fieldName;
            HeaderText = headerText ?? fieldName;
        }


        public bool Visible { get; set; } = true;
        public string FieldName { get; set; } = null!;
        public string? HeaderText { get; set; }
        public string? CssClass { get; set; }
        public string? Width { get; set; }
        public bool Sortable { get; set; } = true;
        public bool Searchable { get; set; } = true;
        public ColumnDataType? DataType { get; set; }

        public DataGridColumn AddCssClass(string cssClass)
        {
            if (cssClass.IsNotEmpty())
            {
                CssClass = CssClass.IsEmpty()
                    ? cssClass.Trim()
                    : $"{CssClass} {cssClass.Trim()}";
            }

            return this;
        }

        public DataGridColumn SetHeader(string headerText)
        {
            HeaderText = headerText;
            return this;
        }

        public DataGridColumn SetWidth(string width)
        {
            Width = width;
            return this;
        }

        public DataGridColumn SetVisible(bool visible)
        {
            Visible = visible;
            return this;
        }

        private DataGridColumn SetDataType(ColumnDataType dataType)
        {
            DataType = dataType;

            switch (dataType)
            {
                case ColumnDataType.Number:
                case ColumnDataType.Boolean:
                    AddCssClass("text-end");
                    break;

                case ColumnDataType.Date:
                case ColumnDataType.DateTime:
                case ColumnDataType.Time:
                    AddCssClass("text-center");
                    break;

                default:
                    AddCssClass("text-start");
                    break;
            }

            return this;
        }
    }
}
