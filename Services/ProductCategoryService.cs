using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services;

public class ProductCategoryService : CrudService<ProductCategory>
{
    public ProductCategoryService(ILogger<ProductCategoryService> logger, IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
    }
}