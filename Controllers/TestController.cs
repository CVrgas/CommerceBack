using CommerceBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TestController( Test service) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.Get(id);
        return result.IsOk ? 
            StatusCode((int)result.Code, result.Entity) : 
            StatusCode((int)result.Code, result.Message);
    }
    
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAll()
    {
        var result = await service.All();
        return result.IsOk ? 
            StatusCode((int)result.Code, result.Entity) : 
            StatusCode((int)result.Code, result.Message);
    }
}