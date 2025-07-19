using CommerceBack.Common.OperationResults;
using CommerceBack.Entities;
using CommerceBack.Services.Base;
using CommerceBack.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CommerceBack.Services;

public class CartService : CrudService<Cart>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CartService> _logger;
    private readonly ICrudService<Product> _productService;
    private readonly ICrudService<CartProduct> _cartProductService;
    private readonly UserService _userService;
    
    public CartService(IUnitOfWork unitOfWork, ILogger<CartService> logger, ICrudService<Product> productService, ICrudService<CartProduct> cartProductService, UserService userService) 
        : base(logger, unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _productService = productService;
        _cartProductService = cartProductService;
        _userService = userService;
    }
    
    public async Task<IReturnObject> AddItemToCart(int userId, int productId, int quantity = 1)
    {
        try
        {
            var userExistsTask =  await _userService.Exist(userId);
            var productExistsTask =  await _productService.Exist(productId);
            
            if(!userExistsTask) return new ReturnObject().NotFound("user not found");
            if(!productExistsTask) return new ReturnObject().NotFound("product not found");
            
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
            _logger.LogError(ex, $"Error in adding item to Cart, userId: {userId}, productId: {productId}, quantity: {quantity}");
            return new ReturnObject().InternalError("Error in adding item to Cart");
        }
    }
    public async Task<IReturnObject> RemoveItemFromCart(int cartItemId)
    {
        try
        {
            var result =  await _cartProductService.Find( cp => cp.Id == cartItemId);
            if (!result.IsOk || result.Entity == null) return new ReturnObject().NotFound();
            await _cartProductService.Delete(result.Entity);
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
            var cpResult = await _cartProductService.Find(cp => cp.CartId == cartId && cp.ProductsId == productId);
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
            var cpResult = await _cartProductService.Find(cp => cp.CartId == cartId && cp.ProductsId == productId);
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

            var createResult = await base.Create(newCart);
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
        return (await base.Find(c => c.UserId == userId)).Entity?.Id ?? 0;
    }
    public async Task<IReturnObject<Cart>> Get(int userId)
    {
        try
        {
            Func<IQueryable<Cart>, IQueryable<Cart>>? includes = u => u.Include(c => c.CartProducts).ThenInclude(cp => cp.Products);
            var result = await base.Find(cart => cart.UserId == userId, includes);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cart for user: {UserId}", userId);
            return new ReturnObject<Cart>().NotFound();
        }
    }
}