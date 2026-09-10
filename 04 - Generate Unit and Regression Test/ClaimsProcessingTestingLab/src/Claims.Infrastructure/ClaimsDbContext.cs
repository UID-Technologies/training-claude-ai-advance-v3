using Claims.Domain;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure;

public sealed class ClaimsDbContext : DbContext
{
    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options) { }

    public DbSet<Claim> Claims => Set<Claim>();
}
