using ShipmentTracker.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<IShipmentService, ShipmentService>();

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program { }
