namespace ShipmentTracker.Application;

public sealed record CreateShipmentRequest(
    string TrackingNumber,
    string Origin,
    string Destination);
