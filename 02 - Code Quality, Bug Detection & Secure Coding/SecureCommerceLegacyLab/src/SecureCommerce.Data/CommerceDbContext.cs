using Microsoft.EntityFrameworkCore;
using SecureCommerce.Domain;

namespace SecureCommerce.Data;
public class CommerceDbContext : DbContext {
    public CommerceDbContext(DbContextOptions<CommerceDbContext> o) : base(o) {}
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
