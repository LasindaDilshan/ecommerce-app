using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Exceptions;
using System.Linq.Expressions;

namespace EcommerceAPI.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;
    private readonly IDiscountCodeService _discountCodeService;

    public CartService(ApplicationDbContext context, IDiscountCodeService discountCodeService)
    {
        _context = context;
        _discountCodeService = discountCodeService;
    }

    #region Private Helper Methods

    private async Task<Cart> GetOrCreateCartAsync(Expression<Func<Cart, bool>> predicate, Func<Cart> createCart)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(predicate);

        if (cart == null)
        {
            cart = createCart();
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        return cart;
    }

    private async Task<Cart> GetCartWithItemsAsync(Expression<Func<Cart, bool>> predicate)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(predicate)
            ?? throw new NotFoundException("Cart");
    }

    private async Task<Cart> GetCartForRemovalAsync(Expression<Func<Cart, bool>> predicate)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(predicate);
    }

    private async Task<CartDto> AddItemToCartAsync(Cart cart, AddToCartRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new ProductNotFoundException(request.ProductId);
        }

        if (product.StockQuantity < request.Quantity)
        {
            throw new InsufficientStockException(product.Name, request.Quantity, product.StockQuantity);
        }

        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            if (existingItem.Quantity > product.StockQuantity)
            {
                throw new InsufficientStockException(existingItem.Product.Name, existingItem.Quantity, existingItem.Product.StockQuantity);
            }
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                AddedAt = DateTime.UtcNow
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstAsync(c => c.Id == cart.Id);

        return MapToCartDto(cart);
    }

    private async Task<CartDto> UpdateItemInCartAsync(Cart cart, int cartItemId, UpdateCartItemRequest request)
    {
        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId)
            ?? throw new CartItemNotFoundException(cartItemId);

        if (cartItem.Product.StockQuantity < request.Quantity)
        {
            throw new InsufficientStockException(cartItem.Product.Name, request.Quantity, cartItem.Product.StockQuantity);
        }

        cartItem.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToCartDto(cart);
    }

    private async Task<bool> RemoveItemFromCartAsync(Cart cart, int cartItemId)
    {
        if (cart == null) return false;

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
        if (cartItem == null) return false;

        cart.CartItems.Remove(cartItem);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<bool> ClearCartItemsAsync(Cart cart)
    {
        if (cart == null) return false;

        cart.CartItems.Clear();
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region User Cart Operations

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(
            c => c.UserId == userId,
            () => new Cart { UserId = userId, CreatedAt = DateTime.UtcNow });
        return MapToCartDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(int userId, AddToCartRequest request)
    {
        var cart = await GetOrCreateCartAsync(
            c => c.UserId == userId,
            () => new Cart { UserId = userId, CreatedAt = DateTime.UtcNow });
        return await AddItemToCartAsync(cart, request);
    }

    public async Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cart = await GetCartWithItemsAsync(c => c.UserId == userId);
        return await UpdateItemInCartAsync(cart, cartItemId, request);
    }

    public async Task<bool> RemoveFromCartAsync(int userId, int cartItemId)
    {
        var cart = await GetCartForRemovalAsync(c => c.UserId == userId);
        return await RemoveItemFromCartAsync(cart, cartItemId);
    }

    public async Task<bool> ClearCartAsync(int userId)
    {
        var cart = await GetCartForRemovalAsync(c => c.UserId == userId);
        return await ClearCartItemsAsync(cart);
    }

    #endregion

    #region Guest Cart Operations

    public async Task<CartDto> GetGuestCartAsync(string sessionId)
    {
        var cart = await GetOrCreateCartAsync(
            c => c.SessionId == sessionId,
            () => new Cart { SessionId = sessionId, CreatedAt = DateTime.UtcNow });
        return MapToCartDto(cart);
    }

    public async Task<CartDto> AddToGuestCartAsync(string sessionId, AddToCartRequest request)
    {
        var cart = await GetOrCreateCartAsync(
            c => c.SessionId == sessionId,
            () => new Cart { SessionId = sessionId, CreatedAt = DateTime.UtcNow });
        return await AddItemToCartAsync(cart, request);
    }

    public async Task<CartDto> UpdateGuestCartItemAsync(string sessionId, int cartItemId, UpdateCartItemRequest request)
    {
        var cart = await GetCartWithItemsAsync(c => c.SessionId == sessionId);
        return await UpdateItemInCartAsync(cart, cartItemId, request);
    }

    public async Task<bool> RemoveFromGuestCartAsync(string sessionId, int cartItemId)
    {
        var cart = await GetCartForRemovalAsync(c => c.SessionId == sessionId);
        return await RemoveItemFromCartAsync(cart, cartItemId);
    }

    public async Task<bool> ClearGuestCartAsync(string sessionId)
    {
        var cart = await GetCartForRemovalAsync(c => c.SessionId == sessionId);
        return await ClearCartItemsAsync(cart);
    }

    #endregion

    public async Task<CartDto> MergeGuestCartWithUserCartAsync(string sessionId, int userId)
    {
        // Use transaction to ensure atomic cart merge
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get guest cart
            var guestCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (guestCart == null || !guestCart.CartItems.Any())
            {
                await transaction.CommitAsync();
                // No guest cart or empty, just return user cart
                return await GetCartAsync(userId);
            }

            // Get or create user cart
            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                // Convert guest cart to user cart
                guestCart.UserId = userId;
                guestCart.SessionId = null;
                guestCart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return MapToCartDto(guestCart);
            }

            // Merge items from guest cart to user cart
            foreach (var guestItem in guestCart.CartItems.ToList())
            {
                var existingItem = userCart.CartItems.FirstOrDefault(ci => ci.ProductId == guestItem.ProductId);
                if (existingItem != null)
                {
                    // Merge quantities - check stock first
                    var product = guestItem.Product;
                    var newQuantity = existingItem.Quantity + guestItem.Quantity;
                    existingItem.Quantity = Math.Min(newQuantity, product?.StockQuantity ?? newQuantity);
                }
                else
                {
                    // Move item to user cart
                    var newCartItem = new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = guestItem.ProductId,
                        Quantity = guestItem.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    userCart.CartItems.Add(newCartItem);
                }
            }

            // Delete guest cart and save all changes atomically
            _context.Carts.Remove(guestCart);
            userCart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Reload user cart
            userCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstAsync(c => c.Id == userCart.Id);

            return MapToCartDto(userCart);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private CartDto MapToCartDto(Cart cart)
    {
        var items = cart.CartItems.Select(ci => new CartItemDto
        {
            CartItemId = ci.Id,
            ProductId = ci.ProductId,
            ProductName = ci.Product.Name,
            ProductImage = ci.Product.ImageUrl,
            Price = ci.Product.Price,
            DiscountPrice = ci.Product.DiscountPrice,
            Quantity = ci.Quantity,
            TotalPrice = (ci.Product.DiscountPrice ?? ci.Product.Price) * ci.Quantity,
            AvailableStock = ci.Product.StockQuantity
        }).ToList();

        var subTotal = items.Sum(i => i.TotalPrice);

        return new CartDto
        {
            CartId = cart.Id,
            Items = items,
            SubTotal = subTotal,
            DiscountAmount = cart.DiscountAmount,
            CouponCode = cart.AppliedCouponCode,
            FinalTotal = subTotal - cart.DiscountAmount,
            TotalItems = items.Sum(i => i.Quantity)
        };
    }

    // Coupon application methods

    public async Task<CartDto> ApplyCouponToCartAsync(int userId, string couponCode)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            throw new NotFoundException("Cart");
        }

        await _discountCodeService.ApplyCouponToCartAsync(cart.Id, couponCode);

        // Reload cart
        cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstAsync(c => c.Id == cart.Id);

        return MapToCartDto(cart);
    }

    public async Task<CartDto> ApplyCouponToGuestCartAsync(string sessionId, string couponCode)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart == null)
        {
            throw new NotFoundException("Cart");
        }

        await _discountCodeService.ApplyCouponToCartAsync(cart.Id, couponCode);

        // Reload cart
        cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstAsync(c => c.Id == cart.Id);

        return MapToCartDto(cart);
    }

    public async Task<CartDto> RemoveCouponFromCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            throw new NotFoundException("Cart");
        }

        await _discountCodeService.RemoveCouponFromCartAsync(cart.Id);

        // Reload cart
        cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstAsync(c => c.Id == cart.Id);

        return MapToCartDto(cart);
    }

    public async Task<CartDto> RemoveCouponFromGuestCartAsync(string sessionId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);

        if (cart == null)
        {
            throw new NotFoundException("Cart");
        }

        await _discountCodeService.RemoveCouponFromCartAsync(cart.Id);

        // Reload cart
        cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstAsync(c => c.Id == cart.Id);

        return MapToCartDto(cart);
    }
}
