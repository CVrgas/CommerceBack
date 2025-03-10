using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services;

public class ProductCategoryService : ServiceBase<ProductCategory>
{
    public ProductCategoryService(ILogger<ServiceBase<ProductCategory>> logger, IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
    }
}