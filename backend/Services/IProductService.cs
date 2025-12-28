using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParameters parameters);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    Task<List<ProductDto>> GetFeaturedProductsAsync();
    Task<bool> UpdateProductImageAsync(int id, string imageUrl);
}
