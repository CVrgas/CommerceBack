using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.Repository;

namespace CommerceBack.Services;

public class Test(ILogger<ServiceBase<Product>> logger, IEntityStore<Product> store) : ServiceBase<Product>(logger, store)
{

}