using System.Linq.Expressions;

namespace ManageLife.Base
{
    public interface IReposiotyBase<T> where T : class, IEntityBase, new()
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task<T> GetAsync(string key);
        Task<bool> InsertAsync(T entity);
        Task<bool> UpdateAsync(string key, T entity);
        Task<bool> DeleteAsync(string key);
    }
}