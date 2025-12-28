using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Data;
using EcommerceAPI.DTOs;
using EcommerceAPI.Models;

namespace EcommerceAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        // Use database-level counting instead of loading all products into memory
        return await _context.Categories
            .Where(c => c.IsActive)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive)
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        // Use database-level counting instead of loading all products into memory
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                IsActive = c.IsActive,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.IsActive)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId,
            ProductCount = 0
        };
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            throw new Exception("Category not found");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.ParentCategoryId = request.ParentCategoryId;

        await _context.SaveChangesAsync();

        // Use database-level counting
        var productCount = await _context.Products.CountAsync(p => p.CategoryId == id && p.IsActive);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId,
            ProductCount = productCount
        };
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return false;
        }

        // Check if category has products
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            throw new Exception("Cannot delete category with existing products");
        }

        // Soft delete
        category.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }
}
