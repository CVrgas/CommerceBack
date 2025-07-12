using System.Linq.Expressions;
using AutoMapper;
using CommerceBack.Common;
using CommerceBack.Common.OperationResults;
using CommerceBack.DTOs.Product;
using CommerceBack.Entities;
using CommerceBack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace CommerceBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(ProductService service, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> All(string? query, string? filter, string? orderby, string? orderDirection)
        {
            var normalizedFilterList = new List<string>();
            if (!string.IsNullOrEmpty(filter))
            {
                normalizedFilterList = filter
                    .Split(';')
                    .ToList();
            }

            Expression<Func<Product, bool>> predicate =
                p =>
                    // Si query tiene valor, buscar en Name o Description (case-insensitive)
                    (string.IsNullOrEmpty(query) ||
                     EF.Functions.Like(p.Name!.ToLower(), $"%{query.ToLower()}%") ||
                     EF.Functions.Like(p.Description!.ToLower(), $"%{query.ToLower()}%"))
                    // Si normalizedFilterList tiene elementos, filtrar por nombre de categoría
                    &&
                    (normalizedFilterList.Count == 0 ||
                     (p.Category != null && normalizedFilterList.Contains(p.Category.ToString()!)));

            Func<IQueryable<Product>, IOrderedQueryable<Product>>? order = q => q.OrderBy(p => p.Name!);
            
            var result  = await service.GetAll(predicate, order);

            if (result.Entity != null)
            {
                foreach (var p in result.Entity)
                {
                    p.ImageUrl ??= "https://placehold.co/600x400";
                }
            }
            
            return StatusCode((int)result.Code,
                result.IsOk ? mapper.Map<IEnumerable<ProductDto>>(result.Entity) : result.Message);
        }

        [HttpGet("feature")]
        public async Task<IActionResult> GetFeature(int count = 3)
        {
            var result = await service.GetFeatureProduct(count);
            return StatusCode((int)result.Code, result.IsOk ? mapper.Map<IEnumerable<ProductDto>>(result.Entity) : result.Message);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> Get(int productId)
        {
            var result = await service.GetById(productId);
            if (result.Entity != null) result.Entity.ImageUrl ??= "https://placehold.co/600x400";
            return StatusCode((int)result.Code, result.IsOk ? mapper.Map<ProductDto>(result.Entity!) : result.Message);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Paginated(int pageIndex = 1, int pageSize = 10, string? query = null,
            string? order = null)
        {
            try
            {
                Expression<Func<Product, bool>>? predicate = 
                    p => (query == null || (p.Name.Contains(query) || p.Description.Contains(query)));
                
                Func<IQueryable<Product>, IOrderedQueryable<Product>>? orderBy = order?.ToLower() switch
                {
                    "name" => q => q.OrderBy(p => p.Name),
                    "description" => q => q.OrderBy(p => p.Description),
                    "price" => q => q.OrderBy(p => p.Price),
                    _ => q => q.OrderBy(p => p.Name),
                };
                
                var result = await service.GetPaginated(pageIndex, pageSize, predicate, orderBy, null);
                var mapped = mapper.Map<PaginatedResponse<ProductDto>>(result.Entity);
                return StatusCode((int)result.Code, result.IsOk ? mapped : result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ReturnObjectDefaultMessage.InternalError);
            }
        }

        [HttpGet("[action]")]
        public IActionResult Count(string? query = null)
        {
            var result = service.GetCount();
            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(ProductCreateUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = await service.Create(mapper.Map<Product>(model));
            return StatusCode((int)result.Code, result.IsOk ? mapper.Map<ProductDto>(result.Entity) : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateMany(IEnumerable<ProductCreateUpdateDto> models)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await service.BulkCreate(mapper.Map<IEnumerable<Product>>(models));
            return StatusCode((int)result.Code,
                result.IsOk ? mapper.Map<List<ProductDto>>(result.Entity) : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Update(ProductCreateUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var existedProduct = await service.GetById(model.Id);

            if (!existedProduct.IsOk || existedProduct.Entity == null)
                return StatusCode((int)existedProduct.Code, existedProduct.Message);
 
            var product = existedProduct.Entity;

            product.Name = model.Name ?? product.Name;
            product.Description = model.Description ?? product.Description;
            product.Price = model.Price ?? product.Price;
            product.ImageUrl = model.ImageUrl ?? product.ImageUrl;

            var result = await service.Update(product);

            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateMany(List<ProductCreateUpdateDto> models)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productIds = models.Select(p => p.Id).ToList();
            var productsObject = await service.GetAll(p => productIds.Contains(p.Id));

            if (!productsObject.IsOk)
                return StatusCode((int)productsObject.Code, productsObject.Message);

            var products = productsObject.Entity!.ToList();
            var missingIds = productIds.Except(products.Select(p => p.Id)).ToList();

            if (missingIds.Count != 0)
                return NotFound($"The following products were not found: {Join(", ", missingIds)}");

            var modelDictionary = models.ToDictionary(m => m.Id);

            foreach (var product in products)
            {
                if (!modelDictionary.TryGetValue(product.Id, out var model))
                {
                    return NotFound();
                }

                product.Name = model.Name ?? product.Name;
                product.Description = model.Description ?? product.Description;
                product.Price = model.Price ?? product.Price;
                product.ImageUrl = model.ImageUrl ?? product.ImageUrl;
            }

            var result = await service.BulkUpdate(products);

            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }

        //[Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await service.Delete(id);
            return StatusCode((int)response.Code, response.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Restore(int id)
        {
            var response = await service.RestoreAsync(id);
            return StatusCode((int)response.Code, response.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Disable(int id)
        {
            var response = await service.SoftDeleteAsync(id);
            return StatusCode((int)response.Code, response.Message);
        }

        //[Authorize]
        [HttpPost("[action]/{productId}")]
        public async Task<IActionResult> Rate(int productId, decimal newRate)
        {
            var result = await service.Rate(productId, newRate);
            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetSelector()
        {
            var response = await service.GetAll<object>(p => new { p.Name, p.Id, Categoria = p.CategoryNavigation.Name});
            return response.IsOk ? Ok(response.Entity) : StatusCode((int)response.Code, response.Message);
        }
    }

    public class CustomProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
