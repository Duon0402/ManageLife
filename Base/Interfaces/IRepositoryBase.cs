using System.Linq.Expressions;

namespace ManageLife.Base
{
    public interface IRepositoryBase<T> where T : class
    {
        IQueryable<T> Query(bool asNoTracking = false);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetAsync(string key);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<bool> InsertAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<bool> BulkInsertAsync(IEnumerable<T> entities);
        Task<bool> BulkUpdateAsync(IEnumerable<T> entities);
        Task<bool> BulkDeleteAsync(IEnumerable<T> entities);
        Task<bool> DeleteAllAsync();
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    }
}