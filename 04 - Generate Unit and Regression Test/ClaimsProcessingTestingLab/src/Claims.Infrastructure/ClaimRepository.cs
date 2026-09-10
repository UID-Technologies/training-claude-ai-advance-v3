using Claims.Application;
using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure;

public sealed class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _db;

    public ClaimRepository(ClaimsDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(
        Claim claim,
        CancellationToken cancellationToken = default)
    {
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<Claim?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _db.Claims.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Claim>> GetAllAsync(
        CancellationToken cancellationToken = default) =>
        await _db.Claims.AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
