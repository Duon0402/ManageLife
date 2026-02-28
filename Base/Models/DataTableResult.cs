namespace ManageLife.Base
{
    public class DataTableResult<T> where T : class
    {
        public DataTableResult()
        {
            Draw = -1;
            RecordsTotal = -1;
            RecordsFiltered = -1;
            Data = new List<T>();
        }

        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<T> Data { get; set; }
    }
}
