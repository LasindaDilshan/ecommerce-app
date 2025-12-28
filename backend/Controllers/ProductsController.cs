using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.DTOs;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IProductRecommendationService _recommendationService;

    public ProductsController(
        IProductService productService,
        IFileUploadService fileUploadService,
        IProductRecommendationService recommendationService)
    {
        _productService = productService;
        _fileUploadService = fileUploadService;
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParameters parameters)
    {
        var result = await _productService.GetProductsAsync(parameters);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedProducts()
    {
        var products = await _productService.GetFeaturedProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(product);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var product = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(id, request);
            return Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);

        if (!result)
        {
            return NotFound(new { message = "Product not found" });
        }

        return Ok(new { message = "Product deleted successfully" });
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost("{id}/upload-image")]
    public async Task<IActionResult> UploadProductImage(int id, IFormFile file)
    {
        try
        {
            var imageUrl = await _fileUploadService.UploadFileAsync(file, "products");
            var result = await _productService.UpdateProductImageAsync(id, imageUrl);

            if (!result)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/similar")]
    public async Task<IActionResult> GetSimilarProducts(int id, [FromQuery] int limit = 4)
    {
        var products = await _recommendationService.GetSimilarProductsAsync(id, limit);
        return Ok(products);
    }

    [HttpGet("{id}/customers-also-bought")]
    public async Task<IActionResult> GetCustomersAlsoBought(int id, [FromQuery] int limit = 4)
    {
        var products = await _recommendationService.GetCustomersAlsoBoughtAsync(id, limit);
        return Ok(products);
    }

    [Authorize]
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetPersonalizedRecommendations([FromQuery] int limit = 8)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || userId == 0)
        {
            return Unauthorized(new { message = "User not found" });
        }

        var products = await _recommendationService.GetPersonalizedRecommendationsAsync(userId, limit);
        return Ok(products);
    }
}
