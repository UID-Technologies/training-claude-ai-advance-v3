using System.Collections.Concurrent;

namespace ShipmentTracker.Application;

public sealed class ShipmentService : IShipmentService
{
    private readonly ConcurrentDictionary<Guid, Shipment> _shipments = new();

    public Shipment Create(CreateShipmentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TrackingNumber))
            throw new ArgumentException("Tracking number is required.");

        if (string.IsNullOrWhiteSpace(request.Origin))
            throw new ArgumentException("Origin is required.");

        if (string.IsNullOrWhiteSpace(request.Destination))
            throw new ArgumentException("Destination is required.");

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = request.TrackingNumber.Trim(),
            Origin = request.Origin.Trim(),
            Destination = request.Destination.Trim(),
            Status = "Created",
            CreatedAtUtc = DateTime.UtcNow
        };

        _shipments[shipment.Id] = shipment;
        return shipment;
    }

    public Shipment? Get(Guid id) =>
        _shipments.TryGetValue(id, out var shipment) ? shipment : null;

    public IReadOnlyCollection<Shipment> GetAll() =>
        _shipments.Values.OrderByDescending(x => x.CreatedAtUtc).ToArray();

    public bool UpdateStatus(Guid id, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return false;

        if (!_shipments.TryGetValue(id, out var shipment))
            return false;

        shipment.Status = status.Trim();
        return true;
    }
}
