namespace ManageLife.Base
{
    public class DataGridViewModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public string[] ColumnNames { get; set; }
        public List<DataGridAction> Actions { get; set; } = new List<DataGridAction>();
    }

    public class DataGridAction
    {
        public string ButtonText { get; set; }
        public string CssClass { get; set; }
        public string OnClickFunction { get; set; }
    }
}
