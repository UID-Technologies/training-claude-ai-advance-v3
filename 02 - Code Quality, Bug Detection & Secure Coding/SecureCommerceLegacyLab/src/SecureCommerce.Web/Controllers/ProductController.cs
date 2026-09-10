using Microsoft.AspNetCore.Mvc;
using SecureCommerce.Data;
namespace SecureCommerce.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase {
    private readonly ProductRepository _products;
    public ProductController(ProductRepository products) { _products = products; }

    [HttpGet("search")]
    public IActionResult Search(string q) => Ok(_products.Search(q));
}
