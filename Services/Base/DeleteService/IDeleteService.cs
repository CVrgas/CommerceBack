using CommerceBack.Common.OperationResults;

namespace CommerceBack.Services.Base;

public interface IDeleteService<T> where T : class
{
    Task<IReturnObject<T>> Delete(T entity);
    Task<IReturnObject<IEnumerable<T>>> BulkDelete(IEnumerable<T> entity);
}