using Microsoft.AspNetCore.Mvc;
using SecureCommerce.Services;
namespace SecureCommerce.Web.Controllers;

[ApiController]
[Route("api/files")]
public class UploadController : ControllerBase {
    private readonly FileStorageService _storage;
    public UploadController(FileStorageService storage) { _storage = storage; }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file) {
        // no size/type/name validation
        var path = await _storage.SaveAsync(file.FileName, file.OpenReadStream());
        return Ok(new { path });
    }
}
