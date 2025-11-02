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

        public IEnumerable<T>? DataSource { get; set; }

        public Func<T, Task<T>>? OnInsert { get; set; }
        public Func<T, Task<T>>? OnUpdate { get; set; }
        public Func<T, Task<bool>>? OnDelete { get; set; }

        public string? ControllerUrl { get; set; }
        public string? InsertUrl { get; set; }
        public string? UpdateUrl { get; set; }
        public string? DeleteUrl { get; set; }

        public DataGridViewOptions<T> SetColumns(List<DataGridColumn> columns)
        {
            Columns = columns;
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