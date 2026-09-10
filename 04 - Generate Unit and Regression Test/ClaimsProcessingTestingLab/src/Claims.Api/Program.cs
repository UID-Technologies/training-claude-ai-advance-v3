using Claims.Application;
using Claims.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseInMemoryDatabase("claims-db"));

builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
builder.Services.AddScoped<ClaimValidator>();
builder.Services.AddScoped<ClaimCalculator>();
builder.Services.AddScoped<ClaimService>();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program { }
