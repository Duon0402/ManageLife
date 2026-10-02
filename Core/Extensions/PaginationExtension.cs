using Microsoft.EntityFrameworkCore;

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

        /// <summary>Bản async cho truy vấn DB; pageIndex bắt đầu từ 0.</summary>
        public static async Task<PageList<T>> ToPageListAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize, CancellationToken ct = default)
        {
            var totalItems = await source.CountAsync(ct);
            var items = await source.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync(ct);
            return new PageList<T>(items, totalItems, pageIndex, pageSize);
        }
    }
}
