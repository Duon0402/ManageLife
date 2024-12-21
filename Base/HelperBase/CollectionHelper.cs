namespace ManageLife.Base
{
    public static class CollectionHelper
    {
        public static bool IsEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static bool IsNotEmpty<T>(this ICollection<T> collection)
        {
            return !collection.IsEmpty();
        }
    }
}
