using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Context;

namespace CommerceBack.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private readonly MyDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        public Repository(MyDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }
        
        public int Count(Expression<Func<T, bool>>? predicate)
        {
            return _dbSet.Count(predicate!);
        }

        public async Task<T?> Create(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task Delete(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                
            }
        }

        public Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task Delete(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _dbSet.Remove(entity);
            }

            return Task.CompletedTask;
        }

        public async Task<T?> Get(int id, Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
        {
            var keyProp = _dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;

            if (keyProp == null)
            {
                throw new InvalidOperationException("Primary key not found.");
            }

            var query = _dbSet.AsQueryable();

            if (includes == null) return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, keyProp) == id);
            
            query = includes.Aggregate(query, (current, include) => include(current));
            
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, keyProp) == id);
        }

        public async Task<T?> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
        {
            var query = _dbSet.AsQueryable();

            if (includes == null) return await query.FirstOrDefaultAsync(predicate);
            query = includes.Aggregate(query, (current, include) => include(current));

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> All(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
        {
            var query = _dbSet.AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            if (orderBy != null)
                query = query.OrderBy(orderBy);

            return await query.Where(predicate ?? (p => true)).ToListAsync();
        }

        public Task<T> Update(T entity)
        {
            _dbSet.Update(entity);
            return Task.FromResult(entity);
        }

        public async Task<IEnumerable<T>> CreateRange(IEnumerable<T> entities)
        {
            var enumerable = entities as T[] ?? entities.ToArray();
            await _dbSet.AddRangeAsync(enumerable);
            return enumerable;
        }

        public Task DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task<T?> FindOrCreateAsync(Expression<Func<T, bool>> predicate, T newEntity)
        {
            var exist = await _dbSet.AnyAsync(predicate);
            if (exist) return null;
            await _dbSet.AddAsync(newEntity);
            return newEntity;
        }

        public Task<IEnumerable<T>> UpdateRange(IEnumerable<T> entities)
        {
            var updateRange = entities as T[] ?? entities.ToArray();
            _dbSet.UpdateRange(updateRange);
            return Task.FromResult<IEnumerable<T>>(updateRange);
        }

        public async Task<bool> Exists(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<bool> Exists(int id)
        {
            var keyProp = _dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;

            if (keyProp == null)
            {
                throw new InvalidOperationException("Primary key not found.");
            }

            return await _dbSet.AnyAsync(e => EF.Property<int>(e, keyProp) == id);
        }

        public async Task<PaginatedResponse<T>> GetPagianted(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null)
        {
            var query = _dbSet.AsQueryable();

            var totalCount = query.Count();

            if(predicate != null)
                query = query.Where(predicate);

            if(orderBy != null)
                query = query.OrderBy(orderBy);

            if(includes != null)
            {
                query = includes.Aggregate(query, (current, include) => include(current));
            }

            var filtered = query.Count();
            var entities = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResponse<T>()
            {
                PageNumber = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalFiltered = filtered,
                Entities = entities
            };
        }
	}
}
