using SecureCommerce.Services;
namespace SecureCommerce.Tests;

public class DiscountServiceTests {
    [Fact]
    public void Save10_ReturnsTenPercent() {
        var sut = new DiscountService();
        Assert.Equal(100m, sut.CalculateDiscount("SAVE10", 1000m));
    }
}
