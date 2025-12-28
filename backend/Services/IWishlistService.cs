using Backend.DTOs;

namespace Backend.Services;

public interface IWishlistService
{
    Task<WishlistDto> GetWishlistAsync(int userId);
    Task<WishlistDto> AddToWishlistAsync(int userId, AddToWishlistRequest request);
    Task<bool> RemoveFromWishlistAsync(int userId, int wishlistItemId);
    Task<bool> ClearWishlistAsync(int userId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
    Task<WishlistDto> MoveToCartAsync(int userId, MoveToCartRequest request);
}
