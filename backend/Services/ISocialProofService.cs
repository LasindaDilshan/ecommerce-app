using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface ISocialProofService
{
    Task<List<RecentPurchaseDto>> GetRecentPurchasesAsync(int limit = 10);
    Task<ProductSocialProofDto> GetProductSocialProofAsync(int productId);
}
