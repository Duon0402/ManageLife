namespace ManageLife.Core
{
    public static class PaginationExtension
    {
        public static PageList<T> ToPageList<T>(this IQueryable<T> source, int pageIndex, int pageSize)
            => new(source, pageIndex, pageSize);

        public static PageList<T> ToPageList<T>(this IQueryable<T> source)
            => new(source);

        public static PageList<T> ToPageList<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
            => new(source, pageIndex, pageSize);

        public static PageList<T> ToPageList<T>(this IEnumerable<T> source)
            => new(source);
    }
}
