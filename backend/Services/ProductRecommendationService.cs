using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;

namespace EcommerceAPI.Services;

public class ProductRecommendationService : IProductRecommendationService
{
    private readonly ApplicationDbContext _context;

    public ProductRecommendationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetSimilarProductsAsync(int productId, int limit = 4)
    {
        // Get the source product
        var sourceProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (sourceProduct == null)
            return new List<ProductDto>();

        // Find similar products based on:
        // 1. Same category
        // 2. Similar price range (+/- 30%)
        // 3. Active products only
        // 4. Exclude the source product
        var priceMin = sourceProduct.Price * 0.7m;
        var priceMax = sourceProduct.Price * 1.3m;

        var similarProducts = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.Id != productId &&
                       p.IsActive &&
                       p.CategoryId == sourceProduct.CategoryId &&
                       p.Price >= priceMin &&
                       p.Price <= priceMax)
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                StockQuantity = p.StockQuantity,
                SKU = p.SKU,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                AdditionalImages = p.ProductImages.Select(pi => pi.ImageUrl).ToList()
            })
            .ToListAsync();

        // If we don't have enough similar products, fill with other products from the same category
        if (similarProducts.Count < limit)
        {
            var remainingCount = limit - similarProducts.Count;
            var existingIds = similarProducts.Select(p => p.Id).Append(productId).ToList();

            var additionalProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => !existingIds.Contains(p.Id) &&
                           p.IsActive &&
                           p.CategoryId == sourceProduct.CategoryId)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(remainingCount)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    AdditionalImages = p.ProductImages.Select(pi => pi.ImageUrl).ToList()
                })
                .ToListAsync();

            similarProducts.AddRange(additionalProducts);
        }

        return similarProducts;
    }

    public async Task<List<ProductDto>> GetCustomersAlsoBoughtAsync(int productId, int limit = 4)
    {
        // Find products that were bought together with the source product
        // by looking at order items from orders that contain this product
        var relatedProductIds = await _context.OrderItems
            .Where(oi => oi.Order.OrderItems.Any(x => x.ProductId == productId))
            .Where(oi => oi.ProductId != productId)
            .GroupBy(oi => oi.ProductId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(limit)
            .ToListAsync();

        if (!relatedProductIds.Any())
        {
            // Fallback to similar products if no order history exists
            return await GetSimilarProductsAsync(productId, limit);
        }

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => relatedProductIds.Contains(p.Id) && p.IsActive)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                StockQuantity = p.StockQuantity,
                SKU = p.SKU,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                IsFeatured = p.IsFeatured,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                AdditionalImages = p.ProductImages.Select(pi => pi.ImageUrl).ToList()
            })
            .ToListAsync();

        // Maintain the order based on frequency
        var orderedProducts = relatedProductIds
            .Select(id => products.FirstOrDefault(p => p.Id == id))
            .Where(p => p != null)
            .Select(p => p!)
            .ToList();

        return orderedProducts;
    }

    public async Task<List<ProductDto>> GetPersonalizedRecommendationsAsync(int userId, int limit = 8)
    {
        // Get user's order history to find their preferred categories
        var userCategoryPreferences = await _context.OrderItems
            .Where(oi => oi.Order.UserId == userId)
            .Select(oi => oi.Product.CategoryId)
            .GroupBy(cid => cid)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(3)
            .ToListAsync();

        // Get user's previously purchased product IDs to exclude them
        var purchasedProductIds = await _context.OrderItems
            .Where(oi => oi.Order.UserId == userId)
            .Select(oi => oi.ProductId)
            .Distinct()
            .ToListAsync();

        List<ProductDto> recommendations;

        if (userCategoryPreferences.Any())
        {
            // Recommend products from user's preferred categories that they haven't bought
            recommendations = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => !purchasedProductIds.Contains(p.Id) &&
                           p.IsActive &&
                           userCategoryPreferences.Contains(p.CategoryId))
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(limit)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    AdditionalImages = p.ProductImages.Select(pi => pi.ImageUrl).ToList()
                })
                .ToListAsync();
        }
        else
        {
            // For new users with no history, show featured products
            recommendations = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    StockQuantity = p.StockQuantity,
                    SKU = p.SKU,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    AdditionalImages = p.ProductImages.Select(pi => pi.ImageUrl).ToList()
                })
                .ToListAsync();
        }

        return recommendations;
    }
}
