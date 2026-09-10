using Claims.Application;
using Claims.Domain;

namespace Claims.UnitTests;

public sealed class ClaimCalculatorTests
{
    [Fact]
    public void Calculate_Outpatient_ReturnsEightyPercent()
    {
        var calculator = new ClaimCalculator();

        var result = calculator.Calculate(
            TreatmentTypes.Outpatient,
            1_000m,
            false);

        Assert.Equal(800m, result);
    }

    // Intentionally incomplete.
    // Students will use Claude to discover and add the remaining scenarios.
}
