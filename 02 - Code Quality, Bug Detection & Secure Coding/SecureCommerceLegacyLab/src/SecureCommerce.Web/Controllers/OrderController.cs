using Microsoft.AspNetCore.Mvc;
using SecureCommerce.Domain;
using SecureCommerce.Services;
namespace SecureCommerce.Web.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase {
    private readonly OrderService _orders;
    public OrderController(OrderService orders) { _orders = orders; }

    [HttpPost]
    public IActionResult Place(PlaceOrderRequest request) =>
        Ok(new { orderId = _orders.PlaceOrder(request) });
}
