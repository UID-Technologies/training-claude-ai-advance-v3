using ShipmentTracker.Application;

namespace ShipmentTracker.Tests;

public sealed class ShipmentServiceTests
{
    [Fact]
    public void Create_ValidRequest_CreatesShipment()
    {
        var service = new ShipmentService();

        var shipment = service.Create(
            new CreateShipmentRequest("TRK-1001", "Delhi", "Mumbai"));

        Assert.NotEqual(Guid.Empty, shipment.Id);
        Assert.Equal("TRK-1001", shipment.TrackingNumber);
        Assert.Equal("Created", shipment.Status);
    }

    [Fact]
    public void Create_MissingTrackingNumber_Throws()
    {
        var service = new ShipmentService();

        Assert.Throws<ArgumentException>(() =>
            service.Create(new CreateShipmentRequest("", "Delhi", "Mumbai")));
    }

    [Fact]
    public void UpdateStatus_ExistingShipment_UpdatesStatus()
    {
        var service = new ShipmentService();
        var shipment = service.Create(
            new CreateShipmentRequest("TRK-1002", "Pune", "Bengaluru"));

        var updated = service.UpdateStatus(shipment.Id, "InTransit");

        Assert.True(updated);
        Assert.Equal("InTransit", service.Get(shipment.Id)!.Status);
    }
}
