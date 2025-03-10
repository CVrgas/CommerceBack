using AutoMapper;
using CommerceBack.Common.OperationResults;
using CommerceBack.DTOs.Product;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;

namespace CommerceBack.Services
{
    public class ProductService : ServiceBase<Product>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;
        private readonly IMapper _mapper;
        
        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger, IMapper mapper) : base(logger, unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IReturnObject<ProductDto>> RestoreAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.Repository<Product>().GetById(id);
        
                if (product == null) return new ReturnObject<ProductDto>().NotFound("Product not found");
        
                if (product.Disabled) return new ReturnObject<ProductDto>().BadRequest("Product not disabled");
        
                product.Disabled = false;
        
                var updated = await _unitOfWork.Repository<Product>().Update(product);

                return new ReturnObject<ProductDto>().Ok(_mapper.Map<ProductDto>(updated));

            }catch (Exception ex)
            {
                return new ReturnObject<ProductDto>().InternalError(ex.Message);
            }
        }
        
        public async Task<IReturnObject<ProductDto>> SoftDeleteAsync(int id)
        {
            try
            {
                var storedProduct = await _unitOfWork.Repository<Product>().GetById(id);
        
                if (storedProduct == null) return new ReturnObject<ProductDto>().NotFound("product not found");
                
                if (storedProduct.Disabled) return new ReturnObject<ProductDto>().BadRequest("Product already disabled");
        
                storedProduct.Disabled = true;
        
                var product = await _unitOfWork.Repository<Product>().Update(storedProduct);
                        
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
                var product = await _unitOfWork.Repository<Product>().GetById(productId);
        
                if(product == null) return new ReturnObject<decimal>().NotFound();
        
                product.RatingSum += newRate;
                product.RatingCount += 1;
                var averageRating = product.RatingSum / product.RatingCount;
        
                product.Rating = averageRating;
        
                await _unitOfWork.Repository<Product>().Update(product);
                
                return new ReturnObject<decimal>().Ok(Math.Round(averageRating, 2));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while rating product");
                return new ReturnObject<decimal>().InternalError();
            }
        }
    }

}