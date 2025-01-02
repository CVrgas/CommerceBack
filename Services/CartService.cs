using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using Microsoft.EntityFrameworkCore;

namespace CommerceBack.Services;

public class CartService
{
    private readonly ConcreteService<Cart> _cartService;
    private readonly ConcreteService<Product> _productService;
    private readonly ConcreteService<CartProduct> _cartProductService;
    private readonly ConcreteService<User> _userService;
    private readonly ILogger<CartService> _logger;
    
    
    public CartService(ConcreteService<Cart> cartService, ConcreteService<CartProduct> cartProductService, ConcreteService<User> userService, ILogger<CartService> logger, ConcreteService<Product> productService)
    {
        _cartService = cartService;
        _cartProductService = cartProductService;
        _userService = userService;
        _logger = logger;
        _productService = productService;
    }
    public async Task<IReturnObject> AddItemToCart(int userId, int productId, int quantity = 1)
    {
        try
        {
            //TODO 
            // Create enum for message for each type.
            var userExistsTask = _userService.Exist(userId);
            var productExistsTask = _productService.Exist(productId);
            await Task.WhenAll(userExistsTask, productExistsTask);
            
            if(!await userExistsTask) return new ReturnObject().NotFound("user not found");
            if(!await productExistsTask) return new ReturnObject().NotFound("product not found");
            
            var cartResult = await GetOrCreateCartId(userId);
            
            if(!cartResult.IsOk) return new ReturnObject(cartResult.IsOk, cartResult.Message, cartResult.Code);
            
            if (await ProductExistsInCart(cartResult.Entity, productId))
            {
                return await IncreaseQuantity(userId, productId, quantity);
            }
            
            var cartItem = new CartProduct
            {
                CartId = cartResult.Entity,
                ProductsId = productId,
                Quantity = quantity
            };

            var result = await _cartProductService.Create(cartItem);
            return !result.IsOk ? new ReturnObject().InternalError(result.Message) : new ReturnObject().Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in adding item to Cart, cartId: {userId}, productId: {productId}, quantity: {quantity}");
            return new ReturnObject().InternalError("Error in adding item to Cart");
        }
    }
    public async Task<IReturnObject> RemoveItemFromCart(int cartItemId)
    {
        try
        {
            if (!await ProductExistsInCart(cartItemId)) return new ReturnObject().NotFound();
            await _cartProductService.Delete(cartItemId);
            return new ReturnObject().Ok();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from Cart, id: {0}", cartItemId);
            return new ReturnObject().InternalError("Error removing item from Cart");
        }
    }
    public async Task<IReturnObject> IncreaseQuantity(int cartId, int productId, int quantity = 1)
    {
        try
        {
            var cpResult = await _cartProductService.Get(cp => cp.CartId == cartId && cp.ProductsId == productId);
            if (!cpResult.IsOk) return new ReturnObject().NotFound();
            var cp = cpResult.Entity;
            cp!.Quantity += quantity;
            await _cartProductService.Update(cp);
            return new ReturnObject().Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"error in increased quantity, cartId: {cartId}, productId: {productId}, quantity: {quantity}");
            return new ReturnObject().InternalError("error in increased quantity");
        }
    }
    public async Task<IReturnObject> DecreaseQuantity(int cartId, int productId, int quantity = 1)
    {
        try
        {
            var cpResult = await _cartProductService.Get(cp => cp.CartId == cartId && cp.ProductsId == productId);
            if (!cpResult.IsOk) return new ReturnObject().NotFound();
            var cp = cpResult.Entity;
            cp!.Quantity -= quantity;
            
            if (cp.Quantity <= 0)
            {
                await RemoveItemFromCart(cp.Id);
            }
            else
            {
                await _cartProductService.Update(cp);   
            }
            
            return new ReturnObject().Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"error decreasing quantity, cartId: {cartId}, productId: {productId}, quantity: {quantity}");
            return new ReturnObject().InternalError("error decreasing quantity");
        }
    }
    private async Task<bool> ProductExistsInCart(int cartItemId)
    {
        return await _cartProductService.Exist(cartItemId);
    }
    private async Task<bool> ProductExistsInCart(int cartId, int productId)
    {
        return await _cartProductService.Exist(c => c.Id == cartId && c.ProductsId == productId);
    }
    private async Task<IReturnObject<int>> GetOrCreateCartId(int userId)
    {
        try
        {
            // 1. Try to get the user's cart from the database
            var existingCartId = await this.GetCartByUserId(userId);
            if (existingCartId != 0) 
                return new ReturnObject<int>().Ok(existingCartId);

            // 2. Cart doesn't exist, create a new one
            var newCart = new Cart
            {
                UserId = userId,
            };

            var createResult = await _cartService.Create(newCart);
            return !createResult.IsOk ? new ReturnObject<int>(createResult) : new ReturnObject<int>().Ok(createResult.Entity!.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get or create cart for userId: {UserId}", userId);
            return new ReturnObject<int>().InternalError("An error occurred while retrieving or creating the cart.");
        }
    }
    private async Task<int> GetCartByUserId(int userId)
    {
        return await _cartService
            .Query()
            .Where(c => c.UserId == userId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync();
    }
}