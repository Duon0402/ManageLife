using System.Linq.Expressions;

namespace ManageLife.Base
{
    public interface IRepositoryBase<T> where T : class
    {
        IQueryable<T> Query(bool asNoTracking = false);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetAsync(string key);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<bool> InsertAsync(T entity, IUnitOfWork? uow = null);
        Task<bool> UpdateAsync(T entity, IUnitOfWork? uow = null);
        Task<bool> DeleteAsync(T entity, IUnitOfWork? uow = null);
        Task<bool> BulkInsertAsync(IEnumerable<T> entities, IUnitOfWork? uow = null);
        Task<bool> BulkUpdateAsync(IEnumerable<T> entities, IUnitOfWork? uow = null);
        Task<bool> BulkDeleteAsync(IEnumerable<T> entities, IUnitOfWork? uow = null);
        Task<bool> DeleteAllAsync(IUnitOfWork? uow = null);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
    }
}