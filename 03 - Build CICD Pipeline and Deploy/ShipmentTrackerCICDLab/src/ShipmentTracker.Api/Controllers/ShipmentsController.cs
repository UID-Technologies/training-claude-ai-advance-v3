using Microsoft.AspNetCore.Mvc;
using ShipmentTracker.Application;

namespace ShipmentTracker.Api.Controllers;

[ApiController]
[Route("api/shipments")]
public sealed class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipments;

    public ShipmentsController(IShipmentService shipments)
    {
        _shipments = shipments;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<Shipment>> GetAll()
        => Ok(_shipments.GetAll());

    [HttpGet("{id:guid}")]
    public ActionResult<Shipment> Get(Guid id)
    {
        var shipment = _shipments.Get(id);
        return shipment is null ? NotFound() : Ok(shipment);
    }

    [HttpPost]
    public ActionResult<Shipment> Create(CreateShipmentRequest request)
    {
        var shipment = _shipments.Create(request);
        return CreatedAtAction(nameof(Get), new { id = shipment.Id }, shipment);
    }

    [HttpPatch("{id:guid}/status")]
    public IActionResult UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusRequest request)
    {
        return _shipments.UpdateStatus(id, request.Status)
            ? NoContent()
            : NotFound();
    }
}

public sealed record UpdateShipmentStatusRequest(string Status);
