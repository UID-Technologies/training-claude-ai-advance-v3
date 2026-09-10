using Claims.Domain;

namespace Claims.Application;

public sealed class ClaimCalculator
{
    public decimal Calculate(
        string treatmentType,
        decimal claimedAmount,
        bool isEmergency)
    {
        if (isEmergency)
            return decimal.Round(claimedAmount * 0.90m, 2);

        return treatmentType switch
        {
            TreatmentTypes.Outpatient =>
                decimal.Round(claimedAmount * 0.80m, 2),

            // INTENTIONAL TRAINING DEFECT:
            // preventive reimbursement should be capped at 10,000.
            TreatmentTypes.Preventive =>
                claimedAmount,

            TreatmentTypes.Dental =>
                decimal.Round(claimedAmount * 0.60m, 2),

            _ => throw new ArgumentException(
                $"Unsupported treatment type: {treatmentType}")
        };
    }
}
