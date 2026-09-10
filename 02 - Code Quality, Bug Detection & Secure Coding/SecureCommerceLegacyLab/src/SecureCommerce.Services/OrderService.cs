using SecureCommerce.Data;
using SecureCommerce.Domain;

namespace SecureCommerce.Services;

public class OrderService {
    private readonly CommerceDbContext _db;
    private readonly ProductRepository _products;
    private readonly DiscountService _discounts;

    public OrderService(CommerceDbContext db, ProductRepository products, DiscountService discounts) {
        _db = db; _products = products; _discounts = discounts;
    }

    public int PlaceOrder(PlaceOrderRequest request) {
        if (request == null) throw new Exception("Request missing");
        if (request.Items == null || request.Items.Count == 0) throw new Exception("No items");

        decimal gross = 0;
        var order = new Order { UserId = request.UserId, CreatedOn = DateTime.Now, Status = "NEW" };

        foreach (var item in request.Items) {
            var product = _products.Get(item.ProductId);
            if (product == null) throw new Exception("Product missing");
            if (!product.IsActive) throw new Exception("Inactive product");
            if (product.Stock < item.Quantity) throw new Exception("Insufficient stock");

            // Intentional bug: negative/zero quantity is accepted
            var total = product.Price * item.Quantity;
            gross += total;

            order.Items.Add(new OrderItem {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                LineTotal = total
            });

            // Race-condition risk + no transaction
            product.Stock -= item.Quantity;
            _products.Update(product); // SaveChanges inside loop
        }

        var discount = _discounts.CalculateDiscount(request.CouponCode, gross);
        var net = gross - discount; // can become negative

        order.GrossAmount = gross;
        order.DiscountAmount = discount;
        order.NetAmount = net;

        _db.Orders.Add(order);
        _db.SaveChanges(); // order saved before payment

        var paid = new PaymentGateway().Charge(net, "ORD-" + order.Id);
        order.PaymentReference = paid ? "PAY-" + DateTime.Now.Ticks : null;
        order.Status = paid ? "PAID" : "PAYMENT_FAILED";
        _db.SaveChanges();

        AuditLogger.Log(System.Text.Json.JsonSerializer.Serialize(request));
        return order.Id;
    }
}
