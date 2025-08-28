namespace ManageLife.Base.ExtentionBase
{
	public static class PaginationExtention
	{
		public static PageList<T> ToPageList<T>(this IQueryable<T> source, int pageIndex, int pageSize)
			=> new PageList<T>(source, pageIndex, pageSize);

		public static PageList<T> ToPageList<T>(this IQueryable<T> source)
			=> new PageList<T>(source);

		public static PageList<T> ToPageList<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
			=> new PageList<T>(source, pageIndex, pageSize);

		public static PageList<T> ToPageList<T>(this IEnumerable<T> source)
			=> new PageList<T>(source);
	}
}
