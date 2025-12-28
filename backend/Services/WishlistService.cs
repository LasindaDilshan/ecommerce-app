using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;
using EcommerceAPI.Models;
using Backend.Models;
using Backend.DTOs;

namespace Backend.Services;

public class WishlistService : IWishlistService
{
    private readonly ApplicationDbContext _context;
    private readonly ICartService _cartService;

    public WishlistService(ApplicationDbContext context, ICartService cartService)
    {
        _context = context;
        _cartService = cartService;
    }

    public async Task<WishlistDto> GetWishlistAsync(int userId)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.WishlistItems)
            .ThenInclude(wi => wi.Product)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
        {
            // Create wishlist if it doesn't exist
            wishlist = new Wishlist
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
        }

        return MapToWishlistDto(wishlist);
    }

    public async Task<WishlistDto> AddToWishlistAsync(int userId, AddToWishlistRequest request)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.WishlistItems)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Wishlists.Add(wishlist);
        }

        // Check if product already in wishlist
        var existingItem = wishlist.WishlistItems
            .FirstOrDefault(wi => wi.ProductId == request.ProductId);

        if (existingItem != null)
        {
            throw new InvalidOperationException("Product is already in wishlist");
        }

        // Check if product exists
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException("Product not found");
        }

        var wishlistItem = new WishlistItem
        {
            WishlistId = wishlist.Id,
            ProductId = request.ProductId,
            AddedAt = DateTime.UtcNow
        };

        wishlist.WishlistItems.Add(wishlistItem);
        wishlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetWishlistAsync(userId);
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int wishlistItemId)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.WishlistItems)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
        {
            return false;
        }

        var wishlistItem = wishlist.WishlistItems
            .FirstOrDefault(wi => wi.Id == wishlistItemId);

        if (wishlistItem == null)
        {
            return false;
        }

        wishlist.WishlistItems.Remove(wishlistItem);
        wishlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearWishlistAsync(int userId)
    {
        var wishlist = await _context.Wishlists
            .Include(w => w.WishlistItems)
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wishlist == null)
        {
            return false;
        }

        wishlist.WishlistItems.Clear();
        wishlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
    {
        return await _context.WishlistItems
            .AnyAsync(wi => wi.Wishlist.UserId == userId && wi.ProductId == productId);
    }

    public async Task<WishlistDto> MoveToCartAsync(int userId, MoveToCartRequest request)
    {
        var wishlistItem = await _context.WishlistItems
            .Include(wi => wi.Wishlist)
            .Include(wi => wi.Product)
            .FirstOrDefaultAsync(wi => wi.Id == request.WishlistItemId && wi.Wishlist.UserId == userId);

        if (wishlistItem == null)
        {
            throw new InvalidOperationException("Wishlist item not found");
        }

        // Add to cart
        var addToCartRequest = new AddToCartRequest
        {
            ProductId = wishlistItem.ProductId,
            Quantity = request.Quantity
        };

        await _cartService.AddToCartAsync(userId, addToCartRequest);

        // Remove from wishlist
        await RemoveFromWishlistAsync(userId, wishlistItem.Id);

        return await GetWishlistAsync(userId);
    }

    private WishlistDto MapToWishlistDto(Wishlist wishlist)
    {
        return new WishlistDto
        {
            Id = wishlist.Id,
            UserId = wishlist.UserId,
            Items = wishlist.WishlistItems.Select(wi => new WishlistItemDto
            {
                Id = wi.Id,
                ProductId = wi.ProductId,
                ProductName = wi.Product.Name,
                ProductPrice = wi.Product.Price,
                ProductDiscountPrice = wi.Product.DiscountPrice,
                ProductImageUrl = wi.Product.ImageUrl,
                StockQuantity = wi.Product.StockQuantity,
                IsInStock = wi.Product.StockQuantity > 0,
                AddedAt = wi.AddedAt
            }).ToList(),
            ItemCount = wishlist.WishlistItems.Count,
            CreatedAt = wishlist.CreatedAt,
            UpdatedAt = wishlist.UpdatedAt
        };
    }
}
