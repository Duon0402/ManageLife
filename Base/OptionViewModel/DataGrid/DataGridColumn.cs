namespace ManageLife.Base
{
    public class DataGridColumn
    {
        #region Constructors

        public DataGridColumn() { }

        public DataGridColumn(string fieldName, string? title = null)
        {
            FieldName = fieldName;
            Title = title ?? fieldName;
        }

        #endregion

        #region Properties

        public string FieldName { get; set; } = null!;
        public string? Title { get; set; }
        public bool Visible { get; set; } = true;

        public string? CssClass { get; set; }
        public string? Width { get; set; }

        public bool Sortable { get; set; } = true;
        public bool Searchable { get; set; } = true;

        public ColumnDataType? DataType { get; set; }

        #endregion

        #region Fluent Setters

        public DataGridColumn SetTitle(string title)
        {
            Title = title;
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

        public DataGridColumn SetDataType(ColumnDataType dataType)
        {
            DataType = dataType;

            switch (dataType)
            {
                case ColumnDataType.Number:
                case ColumnDataType.Boolean:
                    SetCssClass("text-end");
                    break;

                case ColumnDataType.Date:
                case ColumnDataType.DateTime:
                case ColumnDataType.Time:
                    SetCssClass("text-center");
                    break;

                default:
                    SetCssClass("text-start");
                    break;
            }

            return this;
        }

        public DataGridColumn SetCssClass(string cssClass)
        {
            if (cssClass.IsNotEmpty())
            {
                CssClass = CssClass.IsEmpty()
                    ? cssClass.Trim()
                    : $"{CssClass} {cssClass.Trim()}";
            }

            return this;
        }

        #endregion
    }
}
