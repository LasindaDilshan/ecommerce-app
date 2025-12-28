using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceAPI.Controllers;
using EcommerceAPI.DTOs;
using EcommerceAPI.Exceptions;
using EcommerceAPI.Models;
using EcommerceAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EcommerceAPI.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _productServiceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_productServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetProducts_ShouldReturnOkResultWithProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.99M },
                new Product { Id = 2, Name = "Product 2", Price = 20.99M }
            };
            _productServiceMock.Setup(s => s.GetAllProductsAsync())
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(products);
            _productServiceMock.Verify(s => s.GetAllProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProduct_WithValidId_ShouldReturnOkResultWithProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Price = 15.99M,
                Description = "Test Description",
                Stock = 10
            };
            _productServiceMock.Setup(s => s.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(product);
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var productId = 999;
            _productServiceMock.Setup(s => s.GetProductByIdAsync(productId))
                .ThrowsAsync(new ProductNotFoundException(productId));

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProduct_WithValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "New Description",
                Price = 25.99M,
                Stock = 50,
                CategoryId = 1
            };

            var createdProduct = new Product
            {
                Id = 1,
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                Stock = createDto.Stock,
                CategoryId = createDto.CategoryId
            };

            _productServiceMock.Setup(s => s.CreateProductAsync(It.IsAny<CreateProductDto>()))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.CreateProduct(createDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdResult = result as CreatedAtActionResult;
            createdResult.ActionName.Should().Be(nameof(ProductsController.GetProduct));
            createdResult.RouteValues["id"].Should().Be(createdProduct.Id);
            createdResult.Value.Should().BeEquivalentTo(createdProduct);
        }

        [Fact]
        public async Task UpdateProduct_WithValidData_ShouldReturnOkResult()
        {
            // Arrange
            var productId = 1;
            var updateDto = new UpdateProductDto
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 30.99M,
                Stock = 75
            };

            var updatedProduct = new Product
            {
                Id = productId,
                Name = updateDto.Name,
                Description = updateDto.Description,
                Price = updateDto.Price.Value,
                Stock = updateDto.Stock.Value
            };

            _productServiceMock.Setup(s => s.UpdateProductAsync(productId, It.IsAny<UpdateProductDto>()))
                .ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.UpdateProduct(productId, updateDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(updatedProduct);
        }

        [Fact]
        public async Task UpdateProduct_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var productId = 999;
            var updateDto = new UpdateProductDto { Name = "Updated" };

            _productServiceMock.Setup(s => s.UpdateProductAsync(productId, It.IsAny<UpdateProductDto>()))
                .ThrowsAsync(new ProductNotFoundException(productId));

            // Act
            var result = await _controller.UpdateProduct(productId, updateDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteProduct_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var productId = 1;
            _productServiceMock.Setup(s => s.DeleteProductAsync(productId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _productServiceMock.Verify(s => s.DeleteProductAsync(productId), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var productId = 999;
            _productServiceMock.Setup(s => s.DeleteProductAsync(productId))
                .ThrowsAsync(new ProductNotFoundException(productId));

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task SearchProducts_WithQuery_ShouldReturnOkResultWithMatchingProducts()
        {
            // Arrange
            var query = "test";
            var matchingProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Test Product 1", Price = 10.99M },
                new Product { Id = 2, Name = "Test Product 2", Price = 20.99M }
            };

            _productServiceMock.Setup(s => s.SearchProductsAsync(query))
                .ReturnsAsync(matchingProducts);

            // Act
            var result = await _controller.SearchProducts(query);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(matchingProducts);
        }

        [Fact]
        public async Task GetProductsByCategory_ShouldReturnOkResultWithProducts()
        {
            // Arrange
            var categoryId = 1;
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", CategoryId = categoryId },
                new Product { Id = 2, Name = "Product 2", CategoryId = categoryId }
            };

            _productServiceMock.Setup(s => s.GetProductsByCategoryAsync(categoryId))
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProductsByCategory(categoryId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeEquivalentTo(products);
        }
    }
}