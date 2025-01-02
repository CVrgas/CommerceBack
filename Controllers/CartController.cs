using CommerceBack.Common.OperationResults;
using CommerceBack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommerceBack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : Controller
{
    
    private readonly CartService _cartService;
    private readonly AuthService _authService;
    
    public CartController(CartService cartService, AuthService authService)
    {
        _cartService = cartService;
        _authService = authService;
    }

    [Authorize]
    [HttpPost("[action]")]
    public async Task<IActionResult> AddItemToCart(int productId, int quantity = 1)
    {
        // Get User Id
        var userIdResult = GetUserId();
        
        if(!userIdResult.IsOk) return StatusCode((int)userIdResult.Code, userIdResult.Message);
        var userId = userIdResult.Entity;
        
        var result = await _cartService.AddItemToCart(userId, productId, quantity);
        return StatusCode((int)result.Code, result.Message);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> RemoveItemFromCart(int cartItemId)
    {
        var result = await _cartService.RemoveItemFromCart(cartItemId);
        return StatusCode((int)result.Code, result.Message);
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> IncreaseQuantity(int productId, int quantity = 1)
    {
        // Get User Id
        var userIdResult = GetUserId();
        
        if(!userIdResult.IsOk) return StatusCode((int)userIdResult.Code, userIdResult.Message);
        var userId = userIdResult.Entity;
        
        var result = await _cartService.IncreaseQuantity(userId, productId, quantity);
        return StatusCode((int)result.Code, result.Message);
    }
    
    [HttpPost("[action]")]
    public async Task<IActionResult> DecreaseQuantity(int productId, int quantity = 1)
    {
        // Get User Id
        var userIdResult = GetUserId();
        
        if(!userIdResult.IsOk) return StatusCode((int)userIdResult.Code, userIdResult.Message);
        var userId = userIdResult.Entity;
        
        var result = await _cartService.DecreaseQuantity(userId, productId, quantity);
        return StatusCode((int)result.Code, result.Message);
    }

    private IReturnObject<int> GetUserId()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        return _authService.GetUserIdFromJwt(token);
    }
}