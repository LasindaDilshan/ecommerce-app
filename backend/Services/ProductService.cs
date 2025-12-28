using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParameters parameters)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Where(p => p.IsActive)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(parameters.SearchTerm) ||
                                    p.Description.Contains(parameters.SearchTerm));
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);
        }

        if (parameters.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == parameters.IsFeatured.Value);
        }

        // Apply sorting
        query = parameters.SortBy.ToLower() switch
        {
            "price" => parameters.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Price)
                : query.OrderBy(p => p.Price),
            "name" => parameters.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "createdat" => parameters.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();

        // Get paginated product IDs first
        var paginatedProductIds = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(p => p.Id)
            .ToListAsync();

        // Fetch ratings in a single query to avoid N+1
        var ratings = await _context.ProductRatings
            .Where(pr => paginatedProductIds.Contains(pr.ProductId))
            .ToDictionaryAsync(pr => pr.ProductId, pr => new { pr.AverageRating, pr.TotalReviews });

        // Apply pagination with pre-fetched ratings
        var items = await query
            .Where(p => paginatedProductIds.Contains(p.Id))
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
                AdditionalImages = p.ProductImages.Select(pi => pi.ImageUrl).ToList(),
                Rating = null,
                ReviewCount = 0
            })
            .ToListAsync();

        // Map ratings to products
        foreach (var item in items)
        {
            if (ratings.TryGetValue(item.Id, out var rating))
            {
                item.Rating = rating.AverageRating;
                item.ReviewCount = rating.TotalReviews;
            }
        }

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return null;
        }

        var rating = await _context.ProductRatings
            .Where(pr => pr.ProductId == id)
            .FirstOrDefaultAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            StockQuantity = product.StockQuantity,
            SKU = product.SKU,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            AdditionalImages = product.ProductImages.Select(pi => pi.ImageUrl).ToList(),
            Rating = rating?.AverageRating,
            ReviewCount = rating?.TotalReviews ?? 0
        };
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        // Check if SKU already exists
        if (await _context.Products.AnyAsync(p => p.SKU == request.SKU))
        {
            throw new Exception("Product with this SKU already exists");
        }

        // Check if category exists
        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            StockQuantity = request.StockQuantity,
            SKU = request.SKU,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId,
            IsFeatured = request.IsFeatured,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            StockQuantity = product.StockQuantity,
            SKU = product.SKU,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            CategoryName = category.Name
        };
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            throw new Exception("Product not found");
        }

        // Check if category exists
        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            throw new Exception("Category not found");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountPrice = product.DiscountPrice,
            StockQuantity = product.StockQuantity,
            SKU = product.SKU,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            CategoryName = category.Name
        };
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return false;
        }

        // Soft delete
        product.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ProductDto>> GetFeaturedProductsAsync()
    {
        // Get featured product IDs first
        var featuredProductIds = await _context.Products
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .Select(p => p.Id)
            .ToListAsync();

        // Fetch ratings in a single query to avoid N+1
        var ratings = await _context.ProductRatings
            .Where(pr => featuredProductIds.Contains(pr.ProductId))
            .ToDictionaryAsync(pr => pr.ProductId, pr => new { pr.AverageRating, pr.TotalReviews });

        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => featuredProductIds.Contains(p.Id))
            .OrderByDescending(p => p.CreatedAt)
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
                Rating = null,
                ReviewCount = 0
            })
            .ToListAsync();

        // Map ratings to products
        foreach (var product in products)
        {
            if (ratings.TryGetValue(product.Id, out var rating))
            {
                product.Rating = rating.AverageRating;
                product.ReviewCount = rating.TotalReviews;
            }
        }

        return products;
    }

    public async Task<bool> UpdateProductImageAsync(int id, string imageUrl)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return false;
        }

        product.ImageUrl = imageUrl;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }
}
