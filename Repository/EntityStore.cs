using CommerceBack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Context;

namespace CommerceBack.Repository
{
    public class EntityStore<T> : IEntityStore<T> where T : class
    {

        private readonly MyDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        public EntityStore(MyDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Count(predicate);
        }

        public async Task<T?> Create(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(int Id)
        {
            var entity = _dbSet.Find(Id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(IEnumerable<T> entities)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var entity in entities)
                {
                    _dbSet.Remove(entity);
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<T?> Get(int Id, Func<IQueryable<T>, IQueryable<T>>[] includes = null)
        {
            var keyProp = _dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;

            if (keyProp == null)
            {
                throw new InvalidOperationException("Primary key not found.");
            }

            var query = _dbSet.AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, keyProp) == Id);
        }

        public async Task<T?> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>[] includes = null)
        {
            var query = _dbSet.AsQueryable();

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = include(query);
                }
            }

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

        public IQueryable<T> Querable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<T?> Update(T entity)
        {
            _dbSet.Update(entity);
            await _dbContext.SaveChangesAsync();

            return entity;
        }

        public async Task<IEnumerable<T>> CreateRange(IEnumerable<T> entities)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbSet.AddRangeAsync(entities);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return entities;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteRange(IEnumerable<T> entities)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbSet.RemoveRange(entities);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<T?> FindOrCreateAsync(Expression<Func<T, bool>> predicate, T newEntity)
        {
            var exist = await _dbSet.AnyAsync(predicate);
            if (!exist)
            {
                await _dbSet.AddAsync(newEntity);
                await _dbContext.SaveChangesAsync();
                return newEntity;
            }
            return null;
        }

        public async Task<IEnumerable<T>> UpdateRange(IEnumerable<T> entities)
        {
            var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbSet.UpdateRange(entities);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return entities;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Exists(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<bool> Exists(int Id)
        {
            var keyProp = _dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;

            if (keyProp == null)
            {
                throw new InvalidOperationException("Primary key not found.");
            }

            return await _dbSet.AnyAsync(e => EF.Property<int>(e, keyProp) == Id);
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
                foreach(var include in includes) query = include(query);
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

		public IDbContextTransaction Transaction()
		{
			return _dbContext.Database.BeginTransaction();
		}

		public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
	}
}
