namespace EcommerceAPI.Services;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string filePath);
}
