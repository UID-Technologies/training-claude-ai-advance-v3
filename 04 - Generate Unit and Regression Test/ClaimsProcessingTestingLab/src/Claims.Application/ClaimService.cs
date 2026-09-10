using Claims.Domain;

namespace Claims.Application;

public sealed class ClaimService
{
    private readonly IClaimRepository _repository;
    private readonly INotificationService _notification;
    private readonly ClaimValidator _validator;
    private readonly ClaimCalculator _calculator;

    public ClaimService(
        IClaimRepository repository,
        INotificationService notification,
        ClaimValidator validator,
        ClaimCalculator calculator)
    {
        _repository = repository;
        _notification = notification;
        _validator = validator;
        _calculator = calculator;
    }

    public async Task<Claim> SubmitAsync(
        SubmitClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        _validator.Validate(request, now);

        var payable = _calculator.Calculate(
            request.TreatmentType,
            request.ClaimedAmount,
            request.IsEmergency);

        var claim = new Claim
        {
            Id = Guid.NewGuid(),
            MemberId = request.MemberId.Trim(),
            TreatmentType = request.TreatmentType,
            ClaimedAmount = request.ClaimedAmount,
            PayableAmount = payable,
            TreatmentDate = request.TreatmentDate,
            IsEmergency = request.IsEmergency,
            Status = request.ClaimedAmount >= 100_000m
                ? ClaimStatuses.ManualReview
                : ClaimStatuses.Submitted,
            CreatedAtUtc = now
        };

        // INTENTIONAL TRAINING GAP:
        // there is no duplicate/idempotency check.
        await _repository.AddAsync(claim, cancellationToken);

        await _notification.SendClaimSubmittedAsync(claim, cancellationToken);

        return claim;
    }

    public Task<Claim?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyCollection<Claim>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);
}
