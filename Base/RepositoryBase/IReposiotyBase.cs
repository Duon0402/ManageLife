using System.Linq.Expressions;

namespace ManageLife.Base
{
    public interface IReposiotyBase<T> where T : class, IEntityBase, new()
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<T> GetAsync(string key);
        Task<T> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<bool> InsertAsync(T entity);
        Task<bool> UpdateAsync(T entity, bool isSoftDelete = false);
        Task<bool> DeleteAsync(string key);
    }
}