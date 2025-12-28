using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace EcommerceAPI.Services;

public interface IImageProcessingService
{
    Task<ImageUploadResult> ProcessAndSaveImageAsync(Stream imageStream, string originalFileName);
    void DeleteProductImages(string imageId);
    bool IsValidImageFile(string fileName, long fileSize);
}

public class ImageUploadResult
{
    public string ImageId { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public string LargeUrl { get; set; } = string.Empty;
    public string MediumUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}

public class ImageProcessingService : IImageProcessingService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ImageProcessingService> _logger;

    // Image size configurations
    private const int ThumbnailSize = 150;
    private const int MediumSize = 600;
    private const int LargeSize = 1200;

    // File size limit (10 MB)
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    // Supported extensions
    private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff" };

    public ImageProcessingService(IWebHostEnvironment environment, ILogger<ImageProcessingService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public bool IsValidImageFile(string fileName, long fileSize)
    {
        if (fileSize > MaxFileSizeBytes)
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return SupportedExtensions.Contains(extension);
    }

    public async Task<ImageUploadResult> ProcessAndSaveImageAsync(Stream imageStream, string originalFileName)
    {
        var imageId = Guid.NewGuid().ToString("N");
        var dateFolder = DateTime.UtcNow.ToString("yyyy/MM");

        // Create directory structure
        var uploadBasePath = Path.Combine(_environment.WebRootPath, "uploads", "products", dateFolder);
        Directory.CreateDirectory(uploadBasePath);

        var result = new ImageUploadResult
        {
            ImageId = imageId
        };

        try
        {
            using var image = await Image.LoadAsync(imageStream);

            // Save original (optimized)
            var originalPath = Path.Combine(uploadBasePath, $"{imageId}_original.webp");
            await SaveOptimizedImageAsync(image, originalPath, image.Width, image.Height);
            result.OriginalUrl = $"/uploads/products/{dateFolder}/{imageId}_original.webp";

            // Save large version (max 1200px)
            var largePath = Path.Combine(uploadBasePath, $"{imageId}_large.webp");
            await SaveResizedImageAsync(image, largePath, LargeSize);
            result.LargeUrl = $"/uploads/products/{dateFolder}/{imageId}_large.webp";

            // Save medium version (max 600px)
            var mediumPath = Path.Combine(uploadBasePath, $"{imageId}_medium.webp");
            await SaveResizedImageAsync(image, mediumPath, MediumSize);
            result.MediumUrl = $"/uploads/products/{dateFolder}/{imageId}_medium.webp";

            // Save thumbnail (150x150, cropped to square)
            var thumbnailPath = Path.Combine(uploadBasePath, $"{imageId}_thumb.webp");
            await SaveThumbnailAsync(image, thumbnailPath, ThumbnailSize);
            result.ThumbnailUrl = $"/uploads/products/{dateFolder}/{imageId}_thumb.webp";

            _logger.LogInformation("Successfully processed image {ImageId} from {OriginalFileName}", imageId, originalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image {OriginalFileName}", originalFileName);
            throw;
        }

        return result;
    }

    private async Task SaveOptimizedImageAsync(Image image, string path, int width, int height)
    {
        using var clone = image.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Max
        }));

        var encoder = new WebpEncoder
        {
            Quality = 85,
            FileFormat = WebpFileFormatType.Lossy
        };

        await clone.SaveAsync(path, encoder);
    }

    private async Task SaveResizedImageAsync(Image image, string path, int maxDimension)
    {
        var (newWidth, newHeight) = CalculateNewDimensions(image.Width, image.Height, maxDimension);

        using var clone = image.Clone(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(newWidth, newHeight),
            Mode = ResizeMode.Max
        }));

        var encoder = new WebpEncoder
        {
            Quality = 80,
            FileFormat = WebpFileFormatType.Lossy
        };

        await clone.SaveAsync(path, encoder);
    }

    private async Task SaveThumbnailAsync(Image image, string path, int size)
    {
        // Create a square thumbnail by cropping the center
        using var clone = image.Clone(ctx =>
        {
            // First resize to ensure smallest side is at least 'size' pixels
            var ratio = Math.Max((float)size / image.Width, (float)size / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            ctx.Resize(new ResizeOptions
            {
                Size = new Size(newWidth, newHeight),
                Mode = ResizeMode.Max
            });

            // Then crop to exact square
            var cropX = (newWidth - size) / 2;
            var cropY = (newHeight - size) / 2;

            if (cropX > 0 || cropY > 0)
            {
                ctx.Crop(new Rectangle(
                    Math.Max(0, cropX),
                    Math.Max(0, cropY),
                    Math.Min(size, newWidth),
                    Math.Min(size, newHeight)
                ));
            }
        });

        var encoder = new WebpEncoder
        {
            Quality = 75,
            FileFormat = WebpFileFormatType.Lossy
        };

        await clone.SaveAsync(path, encoder);
    }

    private (int width, int height) CalculateNewDimensions(int originalWidth, int originalHeight, int maxDimension)
    {
        if (originalWidth <= maxDimension && originalHeight <= maxDimension)
        {
            return (originalWidth, originalHeight);
        }

        double ratio;
        if (originalWidth > originalHeight)
        {
            ratio = (double)maxDimension / originalWidth;
        }
        else
        {
            ratio = (double)maxDimension / originalHeight;
        }

        return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
    }

    public void DeleteProductImages(string imageId)
    {
        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products");

            if (!Directory.Exists(uploadsPath))
                return;

            // Search for files matching the imageId pattern
            var files = Directory.GetFiles(uploadsPath, $"{imageId}_*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                File.Delete(file);
                _logger.LogInformation("Deleted image file: {FilePath}", file);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting images for {ImageId}", imageId);
        }
    }
}
