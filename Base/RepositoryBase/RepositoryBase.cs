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
            if (entity is IEntityBase entityBase)
            {
                if (entityBase.Id.IsEmpty())
                    entityBase.Id = IdHeper.NewId();
            }

            if (entity is ICanCreate canCreate)
            {
                if (canCreate.CreatedTime == default)
                    canCreate.CreatedTime = DateTimeHelper.UtcNow();
                if (string.IsNullOrEmpty(canCreate.CreatedUser))
                    canCreate.CreatedUser = GlobalHttpContext.GetUserName() ?? SystemUsers.Unknown;
            }

            await _context.Set<T>().AddAsync(entity);

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

        public async Task<bool> UpdateAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity is ICanUpdate canUpdate)
            {
                if (canUpdate.UpdatedTime == default)
                    canUpdate.UpdatedTime = DateTimeHelper.UtcNow();
                if (string.IsNullOrEmpty(canUpdate.UpdatedUser))
                    canUpdate.UpdatedUser = GlobalHttpContext.GetUserName() ?? SystemUsers.Unknown;
            }

            _context.Entry(entity).State = EntityState.Modified;

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }

        public async Task<bool> DeleteAsync(string key, IUnitOfWork? uow = null)
        {
            var entity = await _context.Set<T>().FindAsync(key);
            if (entity == null)
                return false;

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

            if (uow == null)
            {
                return await _context.SaveChangesAsync() > 0;
            }

            return true;
        }
    }
}