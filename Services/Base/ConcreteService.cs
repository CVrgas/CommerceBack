using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Repository;

namespace CommerceBack.Services.Base;

public class ConcreteService<T> : ServiceBase<T> where T : class
{
    public ConcreteService(ILogger<ServiceBase<T>> logger, IEntityStore<T> store) 
        : base(logger, store) 
    { 
    }
}