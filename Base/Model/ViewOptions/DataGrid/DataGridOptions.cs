namespace ManageLife.Base
{
    public class DataGridOptions<T>
    {
        public DataGridOptions()
        {
            AllowInsert = false;
            AllowUpdate = false;
            AllowDelete = false;

            ShowPaging = false;
            ShowSearching = false;

            DataSource = new DataSourceOptions<T>();
            Columns = new List<DataGridColumnOptions>();
        }

        public bool AllowInsert { get; set; }
        public bool AllowUpdate { get; set; }
        public bool AllowDelete { get; set; }

        public string? Id { get; set; }

        public string? CssClass { get; set; }

        public bool ShowPaging { get; set; }
        public bool ShowSearching { get; set; }

        public List<DataGridColumnOptions> Columns { get; set; }

        public DataSourceOptions<T> DataSource { get; set; }

        public DataGridOptions<T> SetColumns(List<DataGridColumnOptions> columns)
        {
            Columns = columns ?? new List<DataGridColumnOptions>();
            return this;
        }

        public DataGridOptions<T> SetDataSource(DataSourceOptions<T> source)
        {
            DataSource = source ?? throw new ArgumentNullException(nameof(source));
            return this;
        }

        public DataGridOptions<T> SetCssClass(string cssClass)
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