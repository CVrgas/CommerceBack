using CommerceBack.UnitOfWork;

namespace CommerceBack.Services.Base;

public class ConcreteService<T> : ServiceBase<T> where T : class
{
    public ConcreteService(ILogger<ServiceBase<T>> logger, IUnitOfWork unitOfWork) 
        : base(logger, unitOfWork) 
    { 
    }
}