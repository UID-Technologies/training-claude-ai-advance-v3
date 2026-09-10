using Claims.Domain;

namespace Claims.Application;

public interface IClaimRepository
{
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Claim>> GetAllAsync(CancellationToken cancellationToken = default);
}
