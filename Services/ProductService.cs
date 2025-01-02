using CommerceBack.Repository;
using System.Linq.Expressions;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.DTOs.Product;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace CommerceBack.Services
{
    public class ProductService : ServiceBase<Product>
    {
        private readonly IEntityStore<ProductCategory> _productCategoryStore;
        private readonly IEntityStore<Product> _store;
        private readonly ILogger<ProductService> _logger;
        
        public ProductService(IEntityStore<Product> store, ILogger<ProductService> logger, IEntityStore<ProductCategory> productCategoryStore) : base(logger, store)
        {
            _store = store;
            _logger = logger;
            _productCategoryStore = productCategoryStore;
        }
        
        public async Task<IReturnObject<IEnumerable<Product>>> All(string? query, string? orderBy, string? direction, IEnumerable<string>? includes)
        {
            try
            {
                Expression<Func<Product, bool>> predicate =
                    !string.IsNullOrWhiteSpace(query)
                        ? p => EF.Functions.Like(p.Name!.ToLower(), $"%{query.ToLower()}%") ||
                               EF.Functions.Like(p.Description!.ToLower(), $"%{query.ToLower()}%") ||
                               EF.Functions.Like( p.CategoryNavigation.Name!.ToLower().Replace(" ", ""), $"%{query.ToLower()}%")
                        : p => true;

                Expression<Func<Product, object>> order = p => p.Name!;

                Func<IQueryable<Product>, IQueryable<Product>>[]? inc = null;

                var result = await this.All(predicate, order, inc);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return new ReturnObject<IEnumerable<Product>>().InternalError("Error getting products");
            }
        }

        public async Task<IReturnObject<PaginatedResponse<Product>>> Paginated(int pageIndex = 1, int pageSize = 10, string? query = null, string? order = null)
        {
            try
            {
                if (pageIndex <= 0) pageIndex = 1;
                Expression<Func<Product, bool>>? predicate = null;
                Expression<Func<Product, object>>? orderby = null;

                if (query != null)
                {
                    predicate = p => p.Name!.Contains(query) || p.Description!.Contains(query);
                }

                if (order != null)
                {
                    orderby = order.ToLower() switch
                    {
                        "name" => p => p.Name!,
                        "description" => p => p.Description!,
                        "price" => p => p.Price!,
                        _ => p => p.Name!,
                    };
                }

                return await Paginated(pageIndex, pageSize, predicate, orderby);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ReturnObject<PaginatedResponse<Product>>().InternalError();
            }
        }
        
        public async Task<IReturnObject<ProductDto>> RestoreAsync(int id)
        {
            try
            {
                var product = await _store.Get(id);
        
                if (product == null) return new ReturnObject<ProductDto>().NotFound("Product not found");
        
                if (product.Disabled) return new ReturnObject<ProductDto>().BadRequest("Product not disabled");
        
                product.Disabled = false;
        
                var updated = await _store.Update(product);
                        
                return updated != null ?  new ReturnObject<ProductDto>().Ok(MapToDto(updated)) : new ReturnObject<ProductDto>().BadRequest("Product not updated");
        
            }catch (Exception ex)
            {
                return new ReturnObject<ProductDto>().InternalError(ex.Message);
            }
        }
        
        public async Task<IReturnObject<ProductDto>> SoftDeleteAsync(int id)
        {
            try
            {
                var storedProduct = await _store.Get(id);
        
                if (storedProduct == null) return new ReturnObject<ProductDto>().NotFound("product not found");
        
                storedProduct.Disabled = true;
        
                var product = await _store.Update(storedProduct);
                        
                return product == null ? new ReturnObject<ProductDto>().BadRequest("Product not updated") : new ReturnObject<ProductDto>().Ok(MapToDto(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting product {Id}", id);
                return new ReturnObject<ProductDto>().InternalError();
            }
        }
        
        public async Task<IReturnObject<decimal>> Rate(int productId, decimal newRate)
        {
            if (newRate < 0 || newRate > 5) return new ReturnObject<decimal>().BadRequest();
        
            try
            {
                var product = await _store.Get(productId);
        
                if(product == null) return new ReturnObject<decimal>().NotFound();
        
                product.RatingSum += newRate;
                product.RatingCount += 1;
                var averageRating = product.RatingSum / product.RatingCount;
        
                product.Rating = averageRating;
        
                var updated = await _store.Update(product);
                return updated != null ?
                    new ReturnObject<decimal>().Ok(Math.Round(averageRating, 2)) :
                    new ReturnObject<decimal>().BadRequest();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while rating product");
                return new ReturnObject<decimal>().InternalError();
            }
        }

        public async Task<IReturnObject<Product>> Create(ProductCreateDto newProduct)
        {

            try
            {
                var productExist = await _store.Exists(p => p.Name.ToLower() == newProduct.Name.ToLower());
                if(productExist) return new ReturnObject<Product>().BadRequest("Product already exists");
                
                var category = await _productCategoryStore.Get(pc => pc.Id == newProduct.Category);
                if(category == null) return new ReturnObject<Product>().BadRequest("Category not found");

                var product = new Product()
                {
                    Name = newProduct.Name,
                    Price = newProduct.Price,
                    Description = newProduct.Description,
                    ImageUrl = newProduct.ImageUrl,
                    Category = category.Id,
                    CategoryNavigation = category,
                    CreateAt = DateTime.UtcNow
                };

                await Create(product);
                return new ReturnObject<Product>().Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new ReturnObject<Product>().InternalError("Error creating product");
            }
        }
        
        public async Task<IReturnObject<IEnumerable<Product>>> BulkCreate(List<ProductCreateDto> newProduct)
        {
            try
            {
                if(newProduct.Any(newp => newp.Category <= 0)) return new ReturnObject<IEnumerable<Product>>().BadRequest($"Category missing on {newProduct.FirstOrDefault(newp => newp.Category <= 0)!.Name}");
                
                var existedProduct = await _store.Get(p => newProduct.Select(newp => newp.Name.ToLower()).Contains(p.Name.ToLower()));
                if(existedProduct != null) return new ReturnObject<IEnumerable<Product>>().BadRequest($"Product already exists, {existedProduct.Name}");
                
                var categories = await _productCategoryStore.All(pc => newProduct.Select(newp => newp.Category).Contains(pc.Id));
                
                if(!categories.Any()) return new ReturnObject<IEnumerable<Product>>().InternalError($"Error occured while adding new products");
                
                var products = newProduct.Select(newp =>
                {
                    return new Product()
                    {
                        Name = newp.Name,
                        Price = newp.Price,
                        Description = newp.Description,
                        ImageUrl = newp.ImageUrl,
                        Category = categories.FirstOrDefault(cat => cat.Id == newp.Category)!.Id,
                        CategoryNavigation = categories.FirstOrDefault(cat => cat.Id == newp.Category)!,
                        CreateAt = DateTime.UtcNow
                    };
                });

                await BulkCreate(products);
                return new ReturnObject<IEnumerable<Product>>().Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return new ReturnObject<IEnumerable<Product>>().InternalError("Error creating product");
            }
        }

        #region Utils
            private Product MapToEntity(ProductDto productDto)
            {
                return new Product()
                {
                    Id = productDto.Id,
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    ImageUrl = productDto.ImageUrl,
                    Rating = productDto.Rating,
                    RatingSum = productDto.RatingSum,
                    RatingCount = productDto.RatingCount,

                };
            }
            private IEnumerable<Product> MapToEntity(IEnumerable<ProductDto> products)
            {
                return products.Select(MapToEntity).ToList();
            }
            private ProductDto MapToDto(Product product)
            {
                return new ProductDto()
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Rating = product.Rating,
                    RatingSum = product.RatingSum,
                    RatingCount = product.RatingCount,
                };
            }
            private IEnumerable<ProductDto> MapToDto(IEnumerable<Product> products)
            {
                return products.Select(MapToDto).ToList();
            }

        #endregion


    }

}