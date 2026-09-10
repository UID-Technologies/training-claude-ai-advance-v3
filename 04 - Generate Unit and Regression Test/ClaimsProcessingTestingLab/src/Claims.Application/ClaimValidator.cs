using Claims.Domain;

namespace Claims.Application;

public sealed class ClaimValidator
{
    public void Validate(SubmitClaimRequest request, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(request.MemberId))
            throw new ArgumentException("Member ID is required.");

        if (request.ClaimedAmount <= 0)
            throw new ArgumentException("Claim amount must be greater than zero.");

        if (request.ClaimedAmount > 500_000m)
            throw new ArgumentException("Claim amount cannot exceed 500000.");

        if (request.TreatmentDate.Date > utcNow.Date)
            throw new ArgumentException("Treatment date cannot be in the future.");

        var supported = new[]
        {
            TreatmentTypes.Outpatient,
            TreatmentTypes.Preventive,
            TreatmentTypes.Dental
        };

        if (!supported.Contains(request.TreatmentType))
            throw new ArgumentException("Unsupported treatment type.");
    }
}
