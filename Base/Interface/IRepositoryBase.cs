using System.Linq.Expressions;

namespace ManageLife.Base
{
    public interface IReposityBase<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<bool> InsertAsync(T entity, IUnitOfWork? uow = null);
        Task<bool> UpdateAsync(T entity, IUnitOfWork? uow = null);
        Task<bool> DeleteAsync(T entity, IUnitOfWork? uow = null);
        Task<bool> BulkInsertAsync(IEnumerable<T> entities, IUnitOfWork? uow = null);
        Task<bool> BulkUpdateAsync(IEnumerable<T> entities, IUnitOfWork? uow = null);
        Task<bool> BulkDeleteAsync(IEnumerable<T> entities, IUnitOfWork? uow = null);
    }
}