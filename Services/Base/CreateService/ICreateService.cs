using CommerceBack.Common.OperationResults;

namespace CommerceBack.Services.Base.CreateService;

public interface ICreateService<T> where T : class
{
    Task<IReturnObject<T>> Create(T entity);
    
    Task<IReturnObject<IEnumerable<T>>> BulkCreate(IEnumerable<T> entity);
}