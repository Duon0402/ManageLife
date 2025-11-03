namespace ManageLife.Base
{
    public class DataGridViewOptions<T>
    {
        public DataGridViewOptions()
        {
            AllowInsert = false;
            AllowUpdate = false;
            AllowDelete = false;

            ShowPaging = false;
            ShowSearching = false;

            DataSource = new DataSourceOptions<T>();
            Columns = new List<DataGridColumn>();
        }

        public bool AllowInsert { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }

        public string? Id { get; set; }

        public string? CssClass { get; set; }

        public bool ShowPaging { get; set; }
        public bool ShowSearching { get; set; }

        public List<DataGridColumn> Columns { get; set; }

        public DataSourceOptions<T> DataSource { get; set; }

        public DataGridViewOptions<T> SetColumns(List<DataGridColumn> columns)
        {
            Columns = columns ?? new List<DataGridColumn>();
            return this;
        }

        public DataGridViewOptions<T> SetDataSource(DataSourceOptions<T> source)
        {
            DataSource = source ?? throw new ArgumentNullException(nameof(source));
            return this;
        }

        public DataGridViewOptions<T> SetCssClass(string cssClass)
        {
            if (cssClass.IsNotEmpty())
            {
                CssClass = CssClass.IsEmpty()
                    ? cssClass.Trim()
                    : $"{CssClass} {cssClass.Trim()}";
            }

            return this;
        }
    }
}