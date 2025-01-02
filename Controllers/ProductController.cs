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
        public async Task<IActionResult> All(string? query, string? orderby, string? orderDirection)
        {
            var result = await service.All(query, orderby, orderDirection, []);
            return StatusCode((int)result.Code,
                result.IsOk ? mapper.Map<IEnumerable<ProductDto>>(result.Entity) : result.Message);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> Get(int productId)
        {
            var result = await service.Get(productId);
            return StatusCode((int)result.Code, result.IsOk ? mapper.Map<ProductDto>(result.Entity!) : result.Message);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Paginated(int pageIndex = 1, int pageSize = 10, string? query = null,
            string? order = null)
        {
            try
            {
                var result = await service.Paginated(pageIndex, pageSize, query, order);
                var x = mapper.Map<PaginatedResponse<ProductDto>>(result.Entity);
                return StatusCode((int)result.Code, result.IsOk ? x : result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ReturnObjectDefaultMessage.InternalError);
            }
        }

        [HttpGet("[action]")]
        public IActionResult Count(string? query = null)
        {
            var result = service.Count();
            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Create(ProductCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await service.Create(model);
            return StatusCode((int)result.Code, result.IsOk ? mapper.Map<ProductDto>(result.Entity) : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> CreateMany(IEnumerable<ProductCreateDto> models)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await service.BulkCreate(models.ToList());
            return StatusCode((int)result.Code,
                result.IsOk ? mapper.Map<List<ProductDto>>(result.Entity) : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Update(ProductUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var productObject = await service.Get(model.Id);

            if (!productObject.IsOk) return StatusCode((int)productObject.Code, productObject.Message);

            var product = productObject.Entity;

            product!.Name = model.Name ?? product.Name;
            product.Description = model.Description ?? product.Description;
            product.Price = model.Price ?? product.Price;
            product.ImageUrl = model.ImageUrl ?? product.ImageUrl;

            var result = await service.Update(product);

            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }

        //[Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateMany(List<ProductUpdateDto> models)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var productIds = models.Select(p => p.Id).ToList();
            var productsObject = await service.All(p => productIds.Contains(p.Id));

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
    }
}
