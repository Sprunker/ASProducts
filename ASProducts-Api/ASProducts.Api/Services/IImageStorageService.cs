namespace ASProducts.Api.Services;

public interface IImageStorageService
{
    Task<string> SaveAsync(int productId, IFormFile file);
    string? GetPhysicalPath(int productId);
    void Delete(int productId);
}
