namespace Claims.Application;

public sealed record SubmitClaimRequest(
    string MemberId,
    string TreatmentType,
    decimal ClaimedAmount,
    DateTime TreatmentDate,
    bool IsEmergency);
