using Microsoft.EntityFrameworkCore;
using SecureCommerce.Data;
using SecureCommerce.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddDbContext<CommerceDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("CommerceDb")));
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<AdminReportRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DiscountService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<FileStorageService>();

var app = builder.Build();
// intentionally missing auth, HTTPS enforcement, exception handler, etc.
app.MapControllers();
app.Run();
