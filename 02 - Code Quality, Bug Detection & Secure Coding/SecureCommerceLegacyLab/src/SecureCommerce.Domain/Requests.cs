namespace SecureCommerce.Domain;

public class RegisterRequest { public string Email { get; set; } public string Password { get; set; } public string DisplayName { get; set; } }
public class LoginRequest { public string Email { get; set; } public string Password { get; set; } }
public class PlaceOrderRequest { public int UserId { get; set; } public string CouponCode { get; set; } public List<PlaceOrderItemRequest> Items { get; set; } }
public class PlaceOrderItemRequest { public int ProductId { get; set; } public int Quantity { get; set; } }
