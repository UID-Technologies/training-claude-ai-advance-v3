namespace ShipmentTracker.Application;

public interface IShipmentService
{
    Shipment Create(CreateShipmentRequest request);
    Shipment? Get(Guid id);
    IReadOnlyCollection<Shipment> GetAll();
    bool UpdateStatus(Guid id, string status);
}
