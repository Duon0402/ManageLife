using System.Diagnostics.CodeAnalysis;

namespace ManageLife.Base
{
    public static class CollectionExtension
    {
        public static bool IsEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? collection)
            => collection == null || !collection.Any();

        public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? collection)
            => collection != null && collection.Any();
    }
}
