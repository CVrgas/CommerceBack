using CommerceBack.Entities;
using CommerceBack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommerceBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController(UserService service) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await service.GetAll();
        return result.IsOk ? StatusCode((int)result.Code, result.Entity) : StatusCode((int)result.Code, result.Message); 
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        Func<IQueryable<User>,IQueryable<User>>[] includes =
        [
            query => query
                .Include(u => u.CartNavigation)
                    .ThenInclude( c => c!.CartProducts),
        ];
        var result = await service.GetById(id, includes);
        return result.IsOk ? StatusCode((int)result.Code, result.Entity) : StatusCode((int)result.Code, result.Message); 
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.Delete(id);
        return result.IsOk ? StatusCode((int)result.Code, result.Entity) : StatusCode((int)result.Code, result.Message);
    }
}