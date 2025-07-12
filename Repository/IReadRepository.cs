using System.Linq.Expressions;
using CommerceBack.Common;

namespace CommerceBack.Repository;

public interface IReadRepository<T> where T : class
{
    Task<T?> GetById(int id, Func<IQueryable<T>, IQueryable<T>>? includes = null);
    Task<TProjection?> GetById<TProjection>(int id, Expression<Func<T, TProjection>> selector);

    Task<T?> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? includes = null);
    Task<TProjection?> Get<TProjection>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProjection>> selector);

    Task<IEnumerable<T>> GetAll(
        Expression<Func<T, bool>>? predicate = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, 
        Func<IQueryable<T>, IQueryable<T>>? includes = null, 
        int? take = null);
        
    Task<IEnumerable<TProjection>> GetAll<TProjection>(
        Expression<Func<T, TProjection>> selector,
        Expression<Func<T, bool>>? predicate = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? take = null);
        
    Task<PaginatedResponse<T>> GetPaginated(
        int pageIndex, 
        int pageSize, 
        Expression<Func<T, bool>>? predicate = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, 
        Func<IQueryable<T>, IQueryable<T>>? includes = null);
        
    Task<PaginatedResponse<TProjection>> GetPaginated<TProjection>(
        int pageIndex, 
        int pageSize,
        Expression<Func<T, TProjection>> selector,
        Expression<Func<T, bool>>? predicate = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    
    int Count(Expression<Func<T, bool>>? predicate = null);
    
    Task<bool> Exists(int id);

    Task<bool> Exists(Expression<Func<T, bool>> predicate);
}