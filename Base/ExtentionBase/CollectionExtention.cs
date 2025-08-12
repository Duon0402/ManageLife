using System.Diagnostics.CodeAnalysis;

namespace ManageLife.Base
{
    public static class CollectionExtention
    {
        public static bool IsEmpty<T>(this ICollection<T>? collection)
        {
            return collection == null || collection.Count == 0;
        }

        public static bool IsNotEmpty<T>(
            [NotNullWhen(true)] this ICollection<T>? collection)
        {
            return collection != null && collection.Count > 0;
        }

        public static bool IsEmpty<T>(this IEnumerable<T>? collection)
        {
            return collection == null || !collection.Any();
        }

        public static bool IsNotEmpty<T>(
            [NotNullWhen(true)] this IEnumerable<T>? collection)
        {
            return collection != null && collection.Any();
        }
    }
}
