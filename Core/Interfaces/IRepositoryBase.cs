using System.Linq.Expressions;

namespace ManageLife.Core
{
    public interface IRepositoryBase<T> where T : class
    {
        IQueryable<T> Query(bool asNoTracking = false);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
        Task<T?> GetAsync(string key, CancellationToken ct = default);
        // Overloads: với includes giữ nguyên params (backward compat), overload ct riêng
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
        Task<bool> InsertAsync(T entity, CancellationToken ct = default);
        Task<bool> UpdateAsync(T entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(T entity, CancellationToken ct = default);
        Task<bool> BulkInsertAsync(IEnumerable<T> entities, CancellationToken ct = default);
        Task<bool> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken ct = default);
        Task<bool> BulkDeleteAsync(IEnumerable<T> entities, CancellationToken ct = default);
        Task<bool> DeleteAllAsync(CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct);
    }
}
