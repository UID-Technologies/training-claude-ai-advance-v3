namespace ShipmentTracker.Application;

public sealed class Shipment
{
    public Guid Id { get; init; }
    public string TrackingNumber { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public string Destination { get; init; } = string.Empty;
    public string Status { get; set; } = "Created";
    public DateTime CreatedAtUtc { get; init; }
}
