namespace ManageLife.Base
{
	//TODO: Chuyển thành Extention Method
	public static class CollectionExtention
	{
		public static bool IsEmpty<T>(this ICollection<T> collection)
		{
			return collection == null || collection.Count == 0;
		}

		public static bool IsNotEmpty<T>(this ICollection<T> collection)
		{
			return !collection.IsEmpty();
		}

		public static bool IsEmpty<T>(this IEnumerable<T> collection)
		{
			return collection == null || !collection.Any();
		}

		public static bool IsNotEmpty<T>(this IEnumerable<T> collection)
		{
			return !collection.IsEmpty();
		}
	}
}
