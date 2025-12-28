using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceAPI.Services;

namespace EcommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageProcessingService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageProcessingService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a product image. Requires Admin role.
    /// Supports: JPG, JPEG, PNG, GIF, BMP, WebP, TIFF
    /// Max size: 10 MB
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult<ImageUploadResult>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        if (!_imageService.IsValidImageFile(file.FileName, file.Length))
        {
            return BadRequest(new { message = "Invalid file. Supported formats: JPG, PNG, GIF, BMP, WebP, TIFF. Max size: 10 MB" });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _imageService.ProcessAndSaveImageAsync(stream, file.FileName);

            _logger.LogInformation("Image uploaded successfully: {ImageId}", result.ImageId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image: {FileName}", file.FileName);
            return StatusCode(500, new { message = "Failed to process image. Please try again." });
        }
    }

    /// <summary>
    /// Upload multiple product images. Requires Admin role.
    /// </summary>
    [HttpPost("upload-multiple")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB total
    public async Task<ActionResult<List<ImageUploadResult>>> UploadMultipleImages(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { message = "No files uploaded" });
        }

        if (files.Count > 10)
        {
            return BadRequest(new { message = "Maximum 10 files allowed per upload" });
        }

        var results = new List<ImageUploadResult>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            if (!_imageService.IsValidImageFile(file.FileName, file.Length))
            {
                errors.Add($"{file.FileName}: Invalid file format or size");
                continue;
            }

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _imageService.ProcessAndSaveImageAsync(stream, file.FileName);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process image: {FileName}", file.FileName);
                errors.Add($"{file.FileName}: Failed to process");
            }
        }

        if (results.Count == 0)
        {
            return BadRequest(new { message = "All uploads failed", errors });
        }

        return Ok(new { results, errors = errors.Count > 0 ? errors : null });
    }

    /// <summary>
    /// Delete product images by image ID. Requires Admin role.
    /// </summary>
    [HttpDelete("{imageId}")]
    [Authorize(Roles = "Admin")]
    public ActionResult DeleteImage(string imageId)
    {
        if (string.IsNullOrWhiteSpace(imageId))
        {
            return BadRequest(new { message = "Image ID is required" });
        }

        try
        {
            _imageService.DeleteProductImages(imageId);
            return Ok(new { message = "Image deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image: {ImageId}", imageId);
            return StatusCode(500, new { message = "Failed to delete image" });
        }
    }
}
