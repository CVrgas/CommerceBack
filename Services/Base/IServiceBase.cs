using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;

namespace CommerceBack.Services.Base;

public interface IServiceBase<T> where T : class
{
    Task<IReturnObject<T>> Get(int id, Func<IQueryable<T>, IQueryable<T>>[]? includes = null!);

    Task<IReturnObject<T>> Get(Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>[]? includes = null!);

    Task<IReturnObject<IEnumerable<T>>> All(Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null);

    Task<IReturnObject<PaginatedResponse<T>>> Paginated(int pageIndex, int pageSize,
        Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>[]? includes = null);

    IReturnObject<int> Count(Expression<Func<T, bool>>? predicate = null);
    
    Task<bool> Exist(Expression<Func<T, bool>> predicate);
    
    Task<bool> Exist(int id);
    
    Task<IReturnObject<T>> Create(T entity);
    
    Task<IReturnObject<IEnumerable<T>>> BulkCreate(IEnumerable<T> entity);

    Task<IReturnObject<T>> Update(T entity);
    
    Task<IReturnObject<IEnumerable<T>>> BulkUpdate(IEnumerable<T> entity);

    Task<IReturnObject<T>> Delete(T entity);
    
    Task<IReturnObject<IEnumerable<T>>> BulkDelete(IEnumerable<T> entity);
}