using ManageLife.Data;
using Microsoft.EntityFrameworkCore;
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

        //TODO: Lấy thông tin user thực tế
        public async Task<bool> InsertAsync(T entity, IUnitOfWork? uow = null)
        {
            if (entity is ICanCreate canCreate)
            {
                canCreate.CreatedTime = DateTimeHelper.Now();
                canCreate.CreatedUser = "System"; // Lấy từ context user thực tế
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
                canUpdate.UpdatedTime = DateTimeHelper.Now();
                canUpdate.UpdatedUser = "System";
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
            var entity = await _context.Set<T>().FirstOrDefaultAsync(n => n.Id == key);
            if (entity == null)
                return false;

            if (entity is ISoftDelete softDelete)
            {
                softDelete.IsDeleted = true;
                softDelete.DeletedTime = DateTimeHelper.Now();
                softDelete.DeletedUser = "System";
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