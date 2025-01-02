using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductCategoryController(ProductCategoryService service) : Controller
{
    [HttpGet("[action]")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var result = await service.All();
            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
        
    [HttpPost("[action]")]
    public async Task<IActionResult> CreateCategory(string category)
    {
        try
        {
            if (string.IsNullOrEmpty(category)) return BadRequest("Empty category");
            var result = await service.CreateCategory(category);
            return StatusCode((int)result.Code, result.IsOk ? result.Entity : result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ReturnObjectDefaultMessage.InternalError);
        }
    }
}