using Microsoft.Extensions.Logging;
using EcommerceAPI.Exceptions;

namespace EcommerceAPI.Services;

public class FileUploadService : IFileUploadService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadService> _logger;

    // Allowed folder names to prevent path traversal attacks
    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "products", "avatars", "documents", "categories", "reviews"
    };

    public FileUploadService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<FileUploadService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidFileException("File is empty");
        }

        // Validate folder to prevent path traversal attacks
        if (string.IsNullOrEmpty(folder) || !AllowedFolders.Contains(folder))
        {
            throw new InvalidFileException($"Invalid upload folder: {folder}");
        }

        // Additional path traversal protection
        if (folder.Contains("..") || folder.Contains('/') || folder.Contains('\\'))
        {
            throw new InvalidFileException("Invalid folder name");
        }

        var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize");
        if (file.Length > maxFileSize)
        {
            throw new InvalidFileException($"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB");
        }

        var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (allowedExtensions == null || !allowedExtensions.Contains(extension))
        {
            throw new InvalidFileException("File type not allowed");
        }

        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);

        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{folder}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            // Validate path to prevent path traversal
            if (string.IsNullOrEmpty(filePath) || filePath.Contains(".."))
            {
                _logger.LogWarning("Invalid file path for deletion: {FilePath}", filePath);
                return Task.FromResult(false);
            }

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

            // Ensure the path is within the web root
            var resolvedPath = Path.GetFullPath(fullPath);
            var webRootPath = Path.GetFullPath(_environment.WebRootPath);
            if (!resolvedPath.StartsWith(webRootPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Attempted to delete file outside web root: {FilePath}", filePath);
                return Task.FromResult(false);
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }
}
