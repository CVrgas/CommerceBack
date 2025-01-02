using CommerceBack.Services;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using System.Transactions;
using CommerceBack.Common;

namespace CommerceBack.Repository
{
    public interface IEntityStore<T> where T : class
    {
        Task<T?> Create(T entity);

        Task<IEnumerable<T>> CreateRange(IEnumerable<T> entities);

        Task<T?> Get(int Id, Func<IQueryable<T>, IQueryable<T>>[] includes = null);

        Task<T?> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>[] includes = null);

        Task<IEnumerable<T>> All(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null);
        Task<PaginatedResponse<T>> GetPagianted(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null);

        Task<T?> Update(T entity);

        Task<IEnumerable<T>> UpdateRange(IEnumerable<T> entities);

        Task Delete(int Id);

        Task Delete(T entity);

        Task Delete(IEnumerable<T> entities);

        Task DeleteRange(IEnumerable<T> entities);

        Task<bool> Exists(int Id);

        Task<bool> Exists(Expression<Func<T, bool>> predicate);

        Task<T?> FindOrCreateAsync(Expression<Func<T, bool>> predicate, T newEntity);

        int Count(Expression<Func<T, bool>> predicate = null);

        IQueryable<T> Querable();

		IDbContextTransaction Transaction();

		Task SaveChangesAsync();

	}

}
