using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;

namespace CommerceBack.Services.Base;

public interface IReadService<T> where T : class
{
    Task<IReturnObject<IEnumerable<T>>> GetAll(Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null, Func<IQueryable<T>, IQueryable<T>>[]? includes = null);
    Task<IReturnObject<T>> GetById(int id, Func<IQueryable<T>, IQueryable<T>>[]? includes = null!);
    Task<IReturnObject<T>> Find(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>[]? includes = null!);
    
    Task<IReturnObject<PaginatedResponse<T>>> GetPaginated(int pageIndex, int pageSize,
        Expression<Func<T, bool>>? predicate = null, Expression<Func<T, object>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>[]? includes = null);
    
    IReturnObject<int> GetCount(Expression<Func<T, bool>>? predicate = null);
    
    Task<bool> Exist(Expression<Func<T, bool>> predicate);
    
    Task<bool> Exist(int id);
}