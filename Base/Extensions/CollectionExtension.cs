using System.Diagnostics.CodeAnalysis;

namespace ManageLife.Base
{
    public static class CollectionExtension
    {
        public static bool IsEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? collection)
            => collection == null || !collection.Any();

        public static bool IsNotEmpty<T>([NotNullWhen(true)] this IEnumerable<T>? collection)
            => collection != null && collection.Any();

        public static List<TResult> SelectDistinctToList<TSource, TResult>(
            this IEnumerable<TSource> source,
            Func<TSource, TResult> selector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return source
                .Select(selector)
                .Distinct()
                .ToList();
        }

        public static List<TSource> SelectDistinctToList<TSource>(
            this IEnumerable<TSource> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source
                .Distinct()
                .ToList();
        }

        public static bool In<T>(this T item, IEnumerable<T>? collection)
        {
            if (collection == null) return false;
            return collection.Contains(item);
        }

        public static bool NotIn<T>(this T item, IEnumerable<T>? collection)
        {
            return !item.In(collection);
        }

        public static bool NotContains<T>(this IEnumerable<T>? collection, T? item)
        {
            if (collection == null) return true;

            return !collection.Contains(item);
        }
    }
}
