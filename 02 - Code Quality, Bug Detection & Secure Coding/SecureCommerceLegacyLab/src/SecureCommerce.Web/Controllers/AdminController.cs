using Microsoft.AspNetCore.Mvc;
using SecureCommerce.Data;
namespace SecureCommerce.Web.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase {
    private readonly AdminReportRepository _reports;
    public AdminController(AdminReportRepository reports) { _reports = reports; }

    [HttpGet("orders")]
    public async Task<IActionResult> Orders(string status, string fromDate) =>
        Ok(await _reports.SearchOrdersRaw(status, fromDate)); // no authorization
}
