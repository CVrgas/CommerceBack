using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductCategoryController(ProductCategoryService service, ILogger<ProductCategoryController> logger) : Controller
{
    private readonly ILogger<ProductCategoryController> _logger = logger;
    
    [HttpGet("[action]")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var result = await service.GetAll();
            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting the categories");
            return StatusCode(500, ex.Message);
        }
    }
        
    [HttpPost("[action]")]
    public async Task<IActionResult> CreateCategory(string category)
    {
        try
        {
            if (string.IsNullOrEmpty(category)) return BadRequest("Empty category");
            var result = await service.Create(new ProductCategory() { Name = category });
            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, ReturnObjectDefaultMessage.InternalError);
        }
    }
}