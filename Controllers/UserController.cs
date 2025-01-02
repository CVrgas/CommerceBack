using CommerceBack.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(UserService service) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await service.All();
        return result.IsOk ? StatusCode((int)result.Code, result.Entity) : StatusCode((int)result.Code, result.Message); 
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.Get(id);
        return result.IsOk ? StatusCode((int)result.Code, result.Entity) : StatusCode((int)result.Code, result.Message); 
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.Delete(id);
        return result.IsOk ? StatusCode((int)result.Code, result.Entity) : StatusCode((int)result.Code, result.Message);
    }
}