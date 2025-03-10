using CommerceBack.Common.OperationResults;

namespace CommerceBack.Services.Base;

public interface IUpdateService<T> where T : class
{
    Task<IReturnObject<T>> Update(T entity);
    
    Task<IReturnObject<IEnumerable<T>>> BulkUpdate(IEnumerable<T> entity);
    
}