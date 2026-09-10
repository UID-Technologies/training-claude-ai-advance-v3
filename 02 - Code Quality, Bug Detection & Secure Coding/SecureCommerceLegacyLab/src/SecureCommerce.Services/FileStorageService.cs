namespace SecureCommerce.Services;
public class FileStorageService {
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "uploads");

    public async Task<string> SaveAsync(string fileName, Stream content) {
        Directory.CreateDirectory(_root);
        // Intentional path traversal risk
        var path = Path.Combine(_root, fileName);
        using var output = File.Create(path);
        await content.CopyToAsync(output);
        return path;
    }
}
