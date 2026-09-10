namespace Claims.Domain;

public sealed class Claim
{
    public Guid Id { get; set; }
    public string MemberId { get; set; } = string.Empty;
    public string TreatmentType { get; set; } = string.Empty;
    public decimal ClaimedAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public DateTime TreatmentDate { get; set; }
    public bool IsEmergency { get; set; }
    public string Status { get; set; } = ClaimStatuses.Submitted;
    public DateTime CreatedAtUtc { get; set; }
}
