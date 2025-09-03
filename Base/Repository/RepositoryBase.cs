using ManageLife.Commons;
using ManageLife.Data;
using ManageLife.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ManageLife.Base
{
    public class RepositoryBase<T> : IReposiotyBase<T> where T : class
    {
        protected readonly AppDbContext _context;

        public RepositoryBase(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Trả về <see cref="IQueryable{T}"/> cho phép truy vấn LINQ trực tiếp trên bảng tương ứng.
        /// </summary>
        /// 
        public IQueryable<T> Query(bool asNoTracking = false)
        {
            return asNoTracking ? _context.Set<T>().AsNoTracking() : _context.Set<T>();
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

        public async Task<bool> InsertAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity == null)
                return false;

            PrepareEntityForInsert(entity);

            await _context.Set<T>().AddAsync(entity);

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

        public async Task<bool> UpdateAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity == null)
                return false;

            PrepareEntityForUpdate(entity);

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

        public async Task<bool> DeleteAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity == null)
                return false;

            PrepareEntityForDelete(entity);

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

        public async Task<bool> BulkInsertAsync(IEnumerable<T> entities, IUnitOfWork? uow = null)
        {
            if (entities.IsEmpty())
                return false;

            foreach (var entity in entities)
            {
                PrepareEntityForInsert(entity);
            }

            await _context.Set<T>().AddRangeAsync(entities);

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

        public async Task<bool> BulkUpdateAsync(IEnumerable<T> entities, IUnitOfWork? uow = null)
        {
            if (entities.IsEmpty())
                return false;

            foreach (var entity in entities)
            {
                PrepareEntityForUpdate(entity);
            }

            _context.Set<T>().UpdateRange(entities);

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

        public async Task<bool> BulkDeleteAsync(IEnumerable<T> entities, IUnitOfWork? uow = null)
        {
            if (entities.IsEmpty())
                return false;

            foreach (var entity in entities)
            {
                PrepareEntityForDelete(entity);
            }

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

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
                if (canUpdate.UpdatedTime == default)
                    canUpdate.UpdatedTime = DateTimeHelper.UtcNow();
                if (string.IsNullOrEmpty(canUpdate.UpdatedUser))
                    canUpdate.UpdatedUser = GlobalHttpContext.GetUserName() ?? SystemUsers.Unknown;
            }

            _context.Entry(entity).State = EntityState.Modified;
        }

        private void PrepareEntityForDelete(T entity)
        {
            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                if (softDelete.DeletedTime == default)
                    softDelete.DeletedTime = DateTimeHelper.UtcNow();

                if (string.IsNullOrEmpty(softDelete.DeletedUser))
                    softDelete.DeletedUser = GlobalHttpContext.GetUserName() ?? SystemUsers.Unknown;

                _context.Entry(entity).State = EntityState.Modified;
            }
            else
            {
                _context.Entry(entity).State = EntityState.Deleted;
            }
        }
    }
}