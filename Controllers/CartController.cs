using AutoMapper;
using CommerceBack.Common.OperationResults;
using CommerceBack.DTOs.Product;
using CommerceBack.Entities;
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
    private readonly IMapper _mapper;
    
    public CartController(CartService cartService, AuthService authService, IMapper mapper)
    {
        _cartService = cartService;
        _authService = authService;
        _mapper = mapper;
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

    [Authorize]
    [HttpGet("[action]")]
    public async Task<IActionResult> GetCartItems()
    {
        var userIdResult = GetUserId();
        
        var cart = await _cartService.Get(userIdResult.Entity);
        if(!cart.IsOk) return StatusCode((int)userIdResult.Code, userIdResult.Message);
        
        var products = cart.Entity?.CartProducts.Select( cp => _mapper.Map<ProductDto>(cp.Products));
        
        return StatusCode((int)cart.Code, products);
    }

    private IReturnObject<int> GetUserId()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        return _authService.GetUserIdFromJwt(token);
    }
}