using ManageLife.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace ManageLife.Base
{
	public class RepositoryBase<T> : IReposiotyBase<T> where T : class, IEntityBase, new()
    {
        protected readonly AppDbContext _context;

        public RepositoryBase(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var entities = await _context.Set<T>().ToListAsync();
            return entities;
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includeProperties != null && includeProperties.Any())
            {
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }

            query = query.Where(predicate);

            return await query.ToListAsync();
        }

		public async Task<T?> GetAsync(string key)
		{
			var entity = await _context.Set<T>().FirstOrDefaultAsync(x => x.Id == key);
            return entity;
		}

		public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();

            if (includeProperties != null && includeProperties.Any())
            {
                query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
            }

            query = query.Where(predicate);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> InsertAsync(T entity)
        {
            if (entity is CanCreate canCreate)
            {
                canCreate.CreatedTime = DateTimeHelper.Now();
                canCreate.CreatedUser = "System"; // Replace with actual user
            }

            await _context.Set<T>().AddAsync(entity);
            var rs = await _context.SaveChangesAsync();
            return rs > 0;
        }


        public async Task<bool> UpdateAsync(T entity, bool isSoftDelete = false)
        {
            if (!isSoftDelete && entity is ICanUpdate canUpdate)
            {
                canUpdate.UpdatedTime = DateTimeHelper.Now();
                canUpdate.UpdatedUser = "System"; // Replace with actual user
            }

            if (isSoftDelete && entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                softDelete.DeletedTime = DateTimeHelper.Now();
                softDelete.DeletedUser = "System"; // Replace with actual user
            }

            EntityEntry entityEntry = _context.Entry<T>(entity);
            entityEntry.State = EntityState.Modified;
            var rs = await _context.SaveChangesAsync();
            return rs > 0;
        }

        public async Task<bool> DeleteAsync(string key)
        {
            var entity = await _context.Set<T>().FirstOrDefaultAsync(n => n.Id == key);
            if (entity != null)
            {
                if (entity is ISoftDelete)
                {
                    return await UpdateAsync(entity, isSoftDelete: true);
                }

                EntityEntry entityEntry = _context.Entry<T>(entity);
                entityEntry.State = EntityState.Deleted;
                var rs = await _context.SaveChangesAsync();
                return rs > 0;
            }
            return false;
        }
    }
}