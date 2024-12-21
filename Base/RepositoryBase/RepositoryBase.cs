using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections;
using System.Linq.Expressions;

namespace ManageLife.Base
{
    public class RepositoryBase<T> : IReposiotyBase<T> where T : class, IEntityBase, new()
    {
        protected readonly DbContext _context;

        public RepositoryBase(DbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var entities = await _context.Set<T>().ToListAsync();
            return entities;
        }

        public async Task<IEnumerable<T>> FindAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();
            query = includeProperties.Aggregate(query, (current, includeProperties) => current.Include(includeProperties));
            var entites = await query.ToListAsync();
            return entites;
        }

        public async Task<T> GetAsync(string key)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == key);
            return entity;
        }

        public async Task<bool> InsertAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            var rs =await _context.SaveChangesAsync();
            return rs > 0;
        }

        public async Task<bool> UpdateAsync(string key, T entity)
        {
            EntityEntry entityEntry = _context.Entry<T>(entity);
            entityEntry.State = EntityState.Modified;
            var rs = await _context.SaveChangesAsync();
            return rs > 0;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(n => n.Id == key);
            if(entity != null)
            {
                EntityEntry entityEntry = _context.Entry<T>(entity);
                entityEntry.State = EntityState.Deleted;
                var rs = await _context.SaveChangesAsync();
                return rs > 0;
            }
            return false;
        }

    }
}