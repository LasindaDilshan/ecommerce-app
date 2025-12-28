using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using EcommerceAPI.Services;
using EcommerceAPI.Data;
using EcommerceAPI.Models;
using EcommerceAPI.DTOs;

namespace Backend.Tests.Unit.Services
{
    public class ProductServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Setup In-Memory Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Create Service
            _productService = new ProductService(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                Description = "Electronic items"
            };
            _context.Categories.Add(category);

            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Name = "Laptop",
                    Description = "High-performance laptop",
                    Price = 1500,
                    StockQuantity = 10,
                    SKU = "LAP001",
                    CategoryId = 1,
                    IsActive = true,
                    IsFeatured = true
                },
                new Product
                {
                    Id = 2,
                    Name = "Mouse",
                    Description = "Wireless mouse",
                    Price = 50,
                    StockQuantity = 100,
                    SKU = "MOU001",
                    CategoryId = 1,
                    IsActive = true,
                    IsFeatured = false
                },
                new Product
                {
                    Id = 3,
                    Name = "Inactive Product",
                    Description = "Inactive",
                    Price = 100,
                    StockQuantity = 0,
                    SKU = "INA001",
                    CategoryId = 1,
                    IsActive = false
                }
            };
            _context.Products.AddRange(products);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOnlyActiveProducts()
        {
            // Arrange
            var parameters = new ProductQueryParameters();

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2); // Only active products
            result.Items.Should().OnlyContain(p => p.IsActive);
        }

        [Fact]
        public async Task GetProducts_WithSearchTerm_ShouldFilterProducts()
        {
            // Arrange
            var parameters = new ProductQueryParameters
            {
                SearchTerm = "Laptop"
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task GetProducts_WithPriceFilter_ShouldReturnMatchingProducts()
        {
            // Arrange
            var parameters = new ProductQueryParameters
            {
                MinPrice = 100,
                MaxPrice = 2000
            };

            // Act
            var result = await _productService.GetProductsAsync(parameters);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().OnlyContain(p => p.Price >= 100 && p.Price <= 2000);
        }

        [Fact]
        public async Task GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Name.Should().Be("Laptop");
            result.Price.Should().Be(1500);
        }

        [Fact]
        public async Task GetProductById_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _productService.GetProductByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateProduct_ShouldAddProductToDatabase()
        {
            // Arrange
            var createRequest = new CreateProductRequest
            {
                Name = "New Product",
                Description = "New Description",
                Price = 200,
                StockQuantity = 50,
                SKU = "NEW001",
                CategoryId = 1,
                IsFeatured = false
            };

            // Act
            var result = await _productService.CreateProductAsync(createRequest);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createRequest.Name);
            result.Price.Should().Be(createRequest.Price);

            // Verify in database
            var productInDb = await _context.Products.FirstOrDefaultAsync(p => p.SKU == "NEW001");
            productInDb.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateProduct_ShouldModifyExistingProduct()
        {
            // Arrange
            var updateRequest = new UpdateProductRequest
            {
                Name = "Updated Laptop",
                Description = "Updated Description",
                Price = 1800,
                StockQuantity = 15,
                CategoryId = 1,
                IsActive = true,
                IsFeatured = true
            };

            // Act
            var result = await _productService.UpdateProductAsync(1, updateRequest);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Updated Laptop");
            result.Price.Should().Be(1800);
        }

        [Fact]
        public async Task DeleteProduct_ShouldDeactivateProduct()
        {
            // Act
            var result = await _productService.DeleteProductAsync(1);

            // Assert
            result.Should().BeTrue();

            // Verify product is deactivated
            var product = await _context.Products.FindAsync(1);
            product.Should().NotBeNull();
            product!.IsActive.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
