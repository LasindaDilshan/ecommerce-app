using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using EcommerceAPI.DTOs;
using Backend.Tests.Integration.TestFixtures;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Tests.Integration.Controllers
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactoryFixture>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactoryFixture _factory;

        public ApiIntegrationTests(WebApplicationFactoryFixture factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();

            // Seed test data
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EcommerceAPI.Data.ApplicationDbContext>();
                if (!context.Users.Any())
                {
                    WebApplicationFactoryFixture.SeedTestData(context);
                }
            }
        }

        #region Auth Tests

        [Fact]
        public async Task Register_WithValidData_ShouldSucceed()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = $"newuser{Guid.NewGuid()}@test.com",
                Password = "Test123!@#",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
            content.Should().NotBeNull();
            content!.Email.Should().Be(request.Email);
            content.FirstName.Should().Be(request.FirstName);
            content.AccessToken.Should().NotBeNullOrEmpty();
            content.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Admin123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<AuthResponse>();
            content.Should().NotBeNull();
            content!.Email.Should().Be("admin@test.com");
            content.Role.Should().Be("Admin");
            content.AccessToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "WrongPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Product Tests

        [Fact]
        public async Task GetProducts_ShouldReturnProducts()
        {
            // Act
            var response = await _client.GetAsync("/api/products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
            content.Should().NotBeNull();
            content!.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Arrange - Get the first product
            var productsResponse = await _client.GetAsync("/api/products");
            var products = await productsResponse.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
            var firstProductId = products!.Items[0].Id;

            // Act
            var response = await _client.GetAsync($"/api/products/{firstProductId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<ProductDto>();
            content.Should().NotBeNull();
            content!.Id.Should().Be(firstProductId);
        }

        [Fact]
        public async Task CreateProduct_AsAdmin_ShouldSucceed()
        {
            // Arrange - Login as admin
            var loginRequest = new LoginRequest
            {
                Email = "admin@test.com",
                Password = "Admin123!"
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

            var createRequest = new CreateProductRequest
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                StockQuantity = 50,
                SKU = $"TEST{Guid.NewGuid().ToString().Substring(0, 8)}",
                CategoryId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", createRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

            var content = await response.Content.ReadFromJsonAsync<ProductDto>();
            content.Should().NotBeNull();
            content!.Name.Should().Be(createRequest.Name);
            content.Price.Should().Be(createRequest.Price);
        }

        [Fact]
        public async Task CreateProduct_WithoutAuth_ShouldFail()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            var createRequest = new CreateProductRequest
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99m,
                StockQuantity = 50,
                SKU = "TEST123",
                CategoryId = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/products", createRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Cart Tests

        private async Task<string> LoginAsUserAsync()
        {
            var loginRequest = new LoginRequest
            {
                Email = "user@test.com",
                Password = "User123!"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return auth!.AccessToken;
        }

        [Fact]
        public async Task GetCart_WithAuth_ShouldReturnCart()
        {
            // Arrange
            var token = await LoginAsUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<CartDto>();
            content.Should().NotBeNull();
            content!.Items.Should().NotBeNull();
        }

        [Fact]
        public async Task AddToCart_WithValidProduct_ShouldSucceed()
        {
            // Arrange
            var token = await LoginAsUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Get a product first
            var productsResponse = await _client.GetAsync("/api/products");
            var products = await productsResponse.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
            var productId = products!.Items[0].Id;

            var addRequest = new AddToCartRequest
            {
                ProductId = productId,
                Quantity = 2
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/cart/add", addRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<CartDto>();
            content.Should().NotBeNull();
            content!.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetCart_WithoutAuth_ShouldFail()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await _client.GetAsync("/api/cart");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        #endregion

        #region Category Tests

        [Fact]
        public async Task GetCategories_ShouldReturnCategories()
        {
            // Act
            var response = await _client.GetAsync("/api/categories");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
            content.Should().NotBeNull();
            content!.Should().NotBeEmpty();
        }

        #endregion

        #region End-to-End Flow Test

        [Fact]
        public async Task CompleteUserFlow_RegisterLoginAddToCartCheckout_ShouldWork()
        {
            // Step 1: Register a new user
            var registerRequest = new RegisterRequest
            {
                Email = $"flowuser{Guid.NewGuid()}@test.com",
                Password = "Test123!@#",
                FirstName = "Flow",
                LastName = "User"
            };
            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();

            // Step 2: Set auth token
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.AccessToken);

            // Step 3: Get products
            var productsResponse = await _client.GetAsync("/api/products");
            productsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var products = await productsResponse.Content.ReadFromJsonAsync<PagedResult<ProductDto>>();
            products!.Items.Should().NotBeEmpty();

            // Step 4: Add product to cart
            var addToCartRequest = new AddToCartRequest
            {
                ProductId = products.Items[0].Id,
                Quantity = 1
            };
            var addToCartResponse = await _client.PostAsJsonAsync("/api/cart/add", addToCartRequest);
            addToCartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 5: Get cart
            var cartResponse = await _client.GetAsync("/api/cart");
            cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var cart = await cartResponse.Content.ReadFromJsonAsync<CartDto>();
            cart!.Items.Should().HaveCount(1);
            cart.SubTotal.Should().BeGreaterThan(0);
        }

        #endregion
    }
}
