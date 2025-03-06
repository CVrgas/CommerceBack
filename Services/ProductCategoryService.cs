using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services;

public class ProductCategoryService : ServiceBase<ProductCategory>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ServiceBase<ProductCategory>> _logger;
    
    public ProductCategoryService(ILogger<ServiceBase<ProductCategory>> logger, IUnitOfWork unitOfWork) : base(logger, unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IReturnObject<ProductCategory>> CreateCategory(string newCategory)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var categoryExist = await _unitOfWork.Repository<ProductCategory>().Exists(c => c.Name.ToLower() == newCategory.ToLower());
            if (categoryExist) return new ReturnObject<ProductCategory>().BadRequest("Category already exists");

            var category = await _unitOfWork.Repository<ProductCategory>().Create(new ProductCategory() { Name = newCategory });
            await _unitOfWork.CommitAsync();

            return category == null ? 
                new ReturnObject<ProductCategory>().BadRequest("Category not created") :
                new ReturnObject<ProductCategory>().Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating category, {newCategory}");
            await _unitOfWork.RollbackAsync();
            return new ReturnObject<ProductCategory>().InternalError("Error creating category");
        }
    }
}