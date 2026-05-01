using ManageLife.Commons;
using ManageLife.Contexts;
using ManageLife.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ManageLife.Core
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly IUnitOfWork _uow;
        protected readonly AppDbContext _context;
        private readonly IUserContext _userContext;

        public RepositoryBase(IUnitOfWork uow, IUserContext userContext)
        {
            _uow = uow;
            _context = uow.Context;
            _userContext = userContext;
        }

        protected async Task<bool> SaveChangesAsync(CancellationToken ct = default)
        {
            if (_uow.AutoSave)
            {
                return await _context.SaveChangesAsync(ct) > 0;
            }

            return true;
        }

        public IQueryable<T> Query(bool asNoTracking = false)
        {
            return asNoTracking ? _context.Set<T>().AsNoTracking() : _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<T>().ToListAsync(ct);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            return await FindAsync(predicate, CancellationToken.None, includes);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync(ct);
        }

        private async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct,
            Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes?.Any() == true)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.Where(predicate).ToListAsync(ct);
        }

        public async Task<T?> GetAsync(string key, CancellationToken ct = default)
        {
            return await _context.Set<T>().FindAsync(new object[] { key }, ct);
        }

        #region CRUD

        public async Task<bool> InsertAsync(T entity, CancellationToken ct = default)
        {
            if (entity == null) return false;

            PrepareEntityForInsert(entity);

            await _context.Set<T>().AddAsync(entity, ct);

            return await SaveChangesAsync(ct);
        }

        public async Task<bool> UpdateAsync(T entity, CancellationToken ct = default)
        {
            if (entity == null) return false;

            PrepareEntityForUpdate(entity);

            _context.Set<T>().Update(entity);

            return await SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteAsync(T entity, CancellationToken ct = default)
        {
            if (entity == null) return false;

            if (entity is ISoftDelete)
            {
                PrepareEntityForDelete(entity);
                _context.Set<T>().Update(entity);
            }
            else
            {
                _context.Set<T>().Remove(entity);
            }

            return await SaveChangesAsync(ct);
        }

        #endregion

        #region BULK

        public async Task<bool> BulkInsertAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            if (entities.IsEmpty()) return false;

            foreach (var entity in entities)
                PrepareEntityForInsert(entity);

            await _context.Set<T>().AddRangeAsync(entities, ct);

            return await SaveChangesAsync(ct);
        }

        public async Task<bool> BulkUpdateAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            if (entities.IsEmpty()) return false;

            foreach (var entity in entities)
                PrepareEntityForUpdate(entity);

            _context.Set<T>().UpdateRange(entities);

            return await SaveChangesAsync(ct);
        }

        public async Task<bool> BulkDeleteAsync(IEnumerable<T> entities, CancellationToken ct = default)
        {
            if (entities.IsEmpty()) return false;

            foreach (var entity in entities)
                PrepareEntityForDelete(entity);

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
                _context.Set<T>().UpdateRange(entities);
            else
                _context.Set<T>().RemoveRange(entities);

            return await SaveChangesAsync(ct);
        }

        #endregion

        #region Helpers

        private void PrepareEntityForInsert(T entity)
        {
            if (entity is IEntityBase entityBase && entityBase.Id.IsEmpty())
            {
                entityBase.Id = IdHelper.NewId();
            }

            if (entity is ICanCreate canCreate)
            {
                if (canCreate.CreatedTime == default)
                    canCreate.CreatedTime = DateTimeHelper.UtcNow();
                if (canCreate.CreatedUser.IsEmpty())
                    canCreate.CreatedUser = _userContext.GetUserName() ?? SystemUsers.Unknown;
            }
        }

        private void PrepareEntityForUpdate(T entity)
        {
            if (entity is ICanUpdate canUpdate)
            {
                canUpdate.UpdatedTime = DateTimeHelper.UtcNow();
                if (canUpdate.UpdatedUser.IsEmpty())
                    canUpdate.UpdatedUser = _userContext.GetUserName() ?? SystemUsers.Unknown;
            }
        }

        private void PrepareEntityForDelete(T entity)
        {
            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                softDelete.DeletedTime = DateTimeHelper.UtcNow();
                if (softDelete.DeletedUser.IsEmpty())
                    softDelete.DeletedUser = _userContext.GetUserName() ?? SystemUsers.Unknown;
            }
        }

        #endregion

        public async Task<bool> DeleteAllAsync(CancellationToken ct = default)
        {
            var entities = await _context.Set<T>()
                .AsNoTracking()
                .ToListAsync(ct);

            if (entities.IsEmpty())
                return true;

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                foreach (var entity in entities)
                    PrepareEntityForDelete(entity);

                _context.Set<T>().UpdateRange(entities);
            }
            else
            {
                _context.Set<T>().RemoveRange(entities);
            }

            return await SaveChangesAsync(ct);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            return await FirstOrDefaultAsync(predicate, CancellationToken.None, includes);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate, ct);
        }

        private async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct,
            Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _context.Set<T>();
            if (includes?.Any() == true)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }

            return await query.FirstOrDefaultAsync(predicate, ct);
        }
    }
}
