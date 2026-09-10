namespace SecureCommerce.Services;
public class DiscountService {
    public decimal CalculateDiscount(string couponCode, decimal gross) {
        if (couponCode == "SAVE10") return gross * 0.10m;
        if (couponCode == "VIP50") return gross * 0.50m;
        if (couponCode == "FLAT100") return 100m;
        return 0m;
    }
}
