using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface IProductRecommendationService
{
    Task<List<ProductDto>> GetSimilarProductsAsync(int productId, int limit = 4);
    Task<List<ProductDto>> GetCustomersAlsoBoughtAsync(int productId, int limit = 4);
    Task<List<ProductDto>> GetPersonalizedRecommendationsAsync(int userId, int limit = 8);
}
