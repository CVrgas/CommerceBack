using CommerceBack.Services.Base.CreateService;

namespace CommerceBack.Services.Base;

public interface ICrudService<T> : IReadService<T>, ICreateService<T>, IUpdateService<T>, IDeleteService<T> where T : class
{
}