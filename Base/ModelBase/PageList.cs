namespace ManageLife.Base
{
    public class PageList<T>
    {
        public PageList(IQueryable<T> source, int pageIndex = PaginationConst.DefaultPageIndex, int pageSize = PaginationConst.DefaultPageSize)
        {
            TotalItems = source.Count();
            PageIndex = pageIndex;
            PageSize = pageSize;
            Items = source.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public PageList(IEnumerable<T> source, int pageIndex = PaginationConst.DefaultPageIndex, int pageSize = PaginationConst.DefaultPageSize)
        {
            var list = source.ToList();
            TotalItems = list.Count;
            PageIndex = pageIndex;
            PageSize = pageSize;
            Items = list.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public List<T> Items { get; private set; } = new List<T>();
        public int PageIndex { get; private set; } 
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex < TotalPages - 1;
    }

}
