using Claims.Application;
using Claims.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Api.Controllers;

[ApiController]
[Route("api/claims")]
public sealed class ClaimsController : ControllerBase
{
    private readonly ClaimService _service;

    public ClaimsController(ClaimService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<Claim>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Claim>> Get(
        Guid id,
        CancellationToken cancellationToken)
    {
        var claim = await _service.GetAsync(id, cancellationToken);
        return claim is null ? NotFound() : Ok(claim);
    }

    [HttpPost]
    public async Task<ActionResult<Claim>> Submit(
        SubmitClaimRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var claim = await _service.SubmitAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = claim.Id }, claim);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
