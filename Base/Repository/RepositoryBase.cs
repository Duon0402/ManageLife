using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ManageLife.Base
{
    public class RepositoryBase<T> : IReposityBase<T> where T : class
    {
        protected readonly AppDbContext _context;

        public RepositoryBase(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query(bool asNoTracking = false)
        {
            return asNoTracking ? _context.Set<T>().AsNoTracking() : _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes?.Any() == true)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.Where(predicate).ToListAsync();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes?.Any() == true)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        #region CRUD

        public async Task<bool> InsertAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity == null) return false;

            PrepareEntityForInsert(entity);

            await _context.Set<T>().AddAsync(entity);

            if (uow == null)
                return await _context.SaveChangesAsync() > 0;

            return true;
        }

        public async Task<bool> UpdateAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity == null) return false;

            PrepareEntityForUpdate(entity);

            _context.Set<T>().Update(entity);

            if (uow == null)
                return await _context.SaveChangesAsync() > 0;

            return true;
        }

        public async Task<bool> DeleteAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity == null) return false;

            if (entity is ISoftDelete)
            {
                PrepareEntityForDelete(entity); // soft delete
                _context.Set<T>().Update(entity);
            }
            else
            {
                _context.Set<T>().Remove(entity); // hard delete
            }

            if (uow == null)
                return await _context.SaveChangesAsync() > 0;

            return true;
        }

        #endregion

        #region BULK

        public async Task<bool> BulkInsertAsync(IEnumerable<T> entities, IUnitOfWork? uow = null)
        {
            if (entities.IsEmpty()) return false;

            foreach (var entity in entities)
                PrepareEntityForInsert(entity);

            await _context.Set<T>().AddRangeAsync(entities);

            if (uow == null)
                return await _context.SaveChangesAsync() > 0;

            return true;
        }

        public async Task<bool> BulkUpdateAsync(IEnumerable<T> entities, IUnitOfWork? uow = null)
        {
            if (entities.IsEmpty()) return false;

            foreach (var entity in entities)
                PrepareEntityForUpdate(entity);

            _context.Set<T>().UpdateRange(entities);

            if (uow == null)
                return await _context.SaveChangesAsync() > 0;

            return true;
        }

        public async Task<bool> BulkDeleteAsync(IEnumerable<T> entities, IUnitOfWork? uow = null)
        {
            if (entities.IsEmpty()) return false;

            foreach (var entity in entities)
                PrepareEntityForDelete(entity);

            // Soft delete thì UpdateRange, hard delete thì RemoveRange
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
                _context.Set<T>().UpdateRange(entities);
            else
                _context.Set<T>().RemoveRange(entities);

            if (uow == null)
                return await _context.SaveChangesAsync() > 0;

            return true;
        }

        #endregion

        #region Helpers

        private void PrepareEntityForInsert(T entity)
        {
            if (entity is IEntityBase entityBase && entityBase.Id.IsEmpty())
            {
                entityBase.Id = IdHeper.NewId();
            }

            if (entity is ICanCreate canCreate)
            {
                if (canCreate.CreatedTime == default)
                    canCreate.CreatedTime = DateTimeHelper.UtcNow();
                if (canCreate.CreatedUser.IsEmpty())
                    canCreate.CreatedUser = GlobalHttpContext.GetUserName() ?? SystemUsers.Unknown;
            }
        }

        private void PrepareEntityForUpdate(T entity)
        {
            if (entity is ICanUpdate canUpdate)
            {
                canUpdate.UpdatedTime = DateTimeHelper.UtcNow();
                if (canUpdate.UpdatedUser.IsEmpty())
                    canUpdate.UpdatedUser = GlobalHttpContext.GetUserName() ?? SystemUsers.Unknown;
            }
        }

        private void PrepareEntityForDelete(T entity)
        {
            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                softDelete.DeletedTime = DateTimeHelper.UtcNow();
                if (softDelete.DeletedUser.IsEmpty())
                    softDelete.DeletedUser = GlobalHttpContext.GetUserName() ?? SystemUsers.Unknown;
            }
        }

        #endregion
    }
}
