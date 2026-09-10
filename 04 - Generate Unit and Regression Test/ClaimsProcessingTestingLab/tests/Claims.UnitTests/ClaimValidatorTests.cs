using Claims.Application;

namespace Claims.UnitTests;

public sealed class ClaimValidatorTests
{
    [Fact]
    public void Validate_ZeroAmount_Throws()
    {
        var validator = new ClaimValidator();

        var request = new SubmitClaimRequest(
            "MEM-1001",
            "Outpatient",
            0m,
            DateTime.UtcNow.Date,
            false);

        Assert.Throws<ArgumentException>(() =>
            validator.Validate(request, DateTime.UtcNow));
    }

    // Intentionally incomplete.
}
