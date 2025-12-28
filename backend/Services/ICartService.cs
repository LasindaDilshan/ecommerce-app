using Backend.DTOs;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface ICartService
{
    // User cart operations
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddToCartAsync(int userId, AddToCartRequest request);
    Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task<bool> RemoveFromCartAsync(int userId, int cartItemId);
    Task<bool> ClearCartAsync(int userId);

    // Guest cart operations
    Task<CartDto> GetGuestCartAsync(string sessionId);
    Task<CartDto> AddToGuestCartAsync(string sessionId, AddToCartRequest request);
    Task<CartDto> UpdateGuestCartItemAsync(string sessionId, int cartItemId, UpdateCartItemRequest request);
    Task<bool> RemoveFromGuestCartAsync(string sessionId, int cartItemId);
    Task<bool> ClearGuestCartAsync(string sessionId);

    // Cart merging
    Task<CartDto> MergeGuestCartWithUserCartAsync(string sessionId, int userId);

    // Coupon operations
    Task<CartDto> ApplyCouponToCartAsync(int userId, string couponCode);
    Task<CartDto> ApplyCouponToGuestCartAsync(string sessionId, string couponCode);
    Task<CartDto> RemoveCouponFromCartAsync(int userId);
    Task<CartDto> RemoveCouponFromGuestCartAsync(string sessionId);
}
