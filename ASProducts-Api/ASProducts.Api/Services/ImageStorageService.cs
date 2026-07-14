namespace ASProducts.Api.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly string _folder;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public ImageStorageService(IWebHostEnvironment env)
    {
        _folder = Path.Combine(env.WebRootPath, "images", "products");
        Directory.CreateDirectory(_folder);
    }

    public async Task<string> SaveAsync(int productId, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Image format not allowed.");

        Delete(productId);

        var fileName = $"{productId}{extension}";
        var fullPath = Path.Combine(_folder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fullPath;
    }

    public string? GetPhysicalPath(int productId)
    {
        return AllowedExtensions
            .Select(ext => Path.Combine(_folder, $"{productId}{ext}"))
            .FirstOrDefault(File.Exists); 
    }

    public void Delete(int productId)
    {
        var existing = GetPhysicalPath(productId);
        if (existing is not null) File.Delete(existing);
    }
}
