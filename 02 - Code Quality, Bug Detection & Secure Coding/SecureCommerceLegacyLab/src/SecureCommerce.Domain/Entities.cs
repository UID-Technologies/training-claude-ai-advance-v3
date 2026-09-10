namespace SecureCommerce.Domain;

public class User {
    public int Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    public string DisplayName { get; set; }
    public bool IsActive { get; set; }
}

public class Product {
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
}

public class Order {
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; }
    public string PaymentReference { get; set; }
    public DateTime CreatedOn { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem {
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
