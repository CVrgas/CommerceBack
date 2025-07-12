using System.Net;
using System.Security.Principal;
using AutoMapper;
using CommerceBack.Common.OperationResults;
using CommerceBack.DTOs.Product;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services
{
    public class ProductService : CrudService<Product>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger, IMapper mapper) : base(logger,
            unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IReturnObject<ProductDto>> RestoreAsync(int id)
        {
            try
            {
                var productResult = await base.GetById(id);
                if (!productResult.IsOk)
                    return new ReturnObject<ProductDto>(false, productResult.Message, productResult.Code);
                if (!productResult.Entity.Disabled)
                    return new ReturnObject<ProductDto>().BadRequest("Product not disabled");

                productResult.Entity.Disabled = false;

                var updated = await base.Update(productResult.Entity);
                if (!updated.IsOk) return new ReturnObject<ProductDto>(false, updated.Message, updated.Code);

                return new ReturnObject<ProductDto>().Ok();

            }
            catch (Exception ex)
            {
                return new ReturnObject<ProductDto>().InternalError(ex.Message);
            }
        }

        public async Task<IReturnObject<ProductDto>> SoftDeleteAsync(int id)
        {
            try
            {
                var storedProduct = await base.GetById(id);
                if (!storedProduct.IsOk)
                    return new ReturnObject<ProductDto>(false, storedProduct.Message, storedProduct.Code);
                if (storedProduct.Entity.Disabled)
                    return new ReturnObject<ProductDto>().BadRequest("Product already disabled");

                storedProduct.Entity.Disabled = true;

                var product = await base.Update(storedProduct.Entity);
                if (!product.IsOk) return new ReturnObject<ProductDto>(false, product.Message, product.Code);

                return new ReturnObject<ProductDto>().Ok(_mapper.Map<ProductDto>(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting product {Id}", id);
                return new ReturnObject<ProductDto>().InternalError();
            }
        }

        public async Task<IReturnObject<decimal>> Rate(int productId, decimal newRate)
        {
            if (newRate is < 0 or > 5) return new ReturnObject<decimal>().BadRequest();

            try
            {
                var productResult = await base.GetById(productId);
                if (!productResult.IsOk)
                    return new ReturnObject<decimal>(false, productResult.Message, productResult.Code);

                productResult.Entity.RatingSum += newRate;
                productResult.Entity.RatingCount += 1;
                productResult.Entity.Rating = productResult.Entity.RatingSum / productResult.Entity.RatingCount;

                var updateResult = await base.Update(productResult.Entity);
                if (!updateResult.IsOk)
                    return new ReturnObject<decimal>(false, updateResult.Message, updateResult.Code);

                return new ReturnObject<decimal>().Ok(Math.Round(updateResult.Entity.Rating, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while rating product");
                return new ReturnObject<decimal>().InternalError();
            }
        }

        public async Task<IReturnObject<IEnumerable<Product>>> GetFeatureProduct(int count)
        {
            var products = await _unitOfWork.Repository<Product>().GetAll(
                orderBy: q => q.OrderByDescending(p => p.RatingCount).ThenByDescending(p => p.Rating),
                take: count);

            return new ReturnObject<IEnumerable<Product>>().Ok(products);
        }


        public async Task<IReturnObject<IEnumerable<ProductAndCategory>>> GetProductsByCategory(int categoryId)
        {
            return await base.GetAll(
                selector: p => new ProductAndCategory{ productId = p.Id, categoryId =  p.CategoryNavigation.Id },
                predicate: p => p.CategoryNavigation.Id == categoryId);
        }
    }

    public class ProductAndCategory
    {
        public int productId { get; set; }
        public int categoryId { get; set; }

    }

}