using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.Repository;

namespace CommerceBack.Services.Base;

public interface IReadService<T> where T : class
{
    Task<IReturnObject<T>> GetById(int id, Func<IQueryable<T>, IQueryable<T>>? includes = null!);
    Task<IReturnObject<TProjection>> GetById<TProjection>(int id, Expression<Func<T, TProjection>> selector);
    Task<IReturnObject<T>> Find(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? includes = null!);
    Task<IReturnObject<TProjection?>> Get<TProjection>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProjection>> selector);
    Task<IReturnObject<IEnumerable<T>>> GetAll(Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>? includes = null);
    Task<IReturnObject<IEnumerable<TProjection>>> GetAll<TProjection>(Expression<Func<T, TProjection>> selector,
        Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<IReturnObject<PaginatedResponse<T>>> GetPaginated(int pageIndex, int pageSize,
        Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? includes = null);
    Task<IReturnObject<PaginatedResponse<TProjection>>> GetPaginated<TProjection>(int pageIndex, int pageSize,
        Expression<Func<T, TProjection>> selector, Expression<Func<T, bool>>? predicate = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    IReturnObject<int> GetCount(Expression<Func<T, bool>>? predicate = null);
    Task<bool> Exist(Expression<Func<T, bool>> predicate);
    Task<bool> Exist(int id);
}