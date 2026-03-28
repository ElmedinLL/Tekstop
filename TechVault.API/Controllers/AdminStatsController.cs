using Asp.Versioning.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechVault.API.Admin;
using TechVault.API.Services;

namespace TechVault.API.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/admin/stats")]
[Authorize(Roles = "Admin")]
public sealed class AdminStatsController(IAdminStatsService adminStatsService) : ControllerBase
{
    /// <summary>Dashboard aggregates: revenue, orders, users, products, and monthly revenue (last 12 UTC months).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminStatsDto>> Get(CancellationToken cancellationToken)
    {
        var stats = await adminStatsService.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
