using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services
{
    public interface IDiscountCodeService
    {
        // Validation and Application
        Task<CouponValidationResponse> ValidateCouponAsync(string code, int? userId, string? guestEmail, decimal cartSubTotal, List<int> productIds);
        Task<decimal> CalculateDiscountAsync(DiscountCode coupon, decimal cartSubTotal, List<CartItem> cartItems);
        Task ApplyCouponToCartAsync(int cartId, string couponCode);
        Task RemoveCouponFromCartAsync(int cartId);

        // Usage Tracking
        Task IncrementUsageAsync(int discountCodeId, int? userId, string? guestEmail, int? orderId, decimal discountAmount);
        Task<int> GetUserUsageCountAsync(int discountCodeId, int? userId, string? guestEmail);

        // Admin CRUD Operations
        Task<DiscountCodeDto> CreateDiscountCodeAsync(CreateDiscountCodeRequest request);
        Task<DiscountCodeDto> UpdateDiscountCodeAsync(int id, UpdateDiscountCodeRequest request);
        Task<bool> DeleteDiscountCodeAsync(int id);
        Task<DiscountCodeDto?> GetDiscountCodeByIdAsync(int id);
        Task<DiscountCodeDto?> GetDiscountCodeByCodeAsync(string code);
        Task<List<DiscountCodeDto>> GetAllDiscountCodesAsync(bool activeOnly = false);
        Task<DiscountCodeStatsDto> GetStatsAsync();
    }
}
