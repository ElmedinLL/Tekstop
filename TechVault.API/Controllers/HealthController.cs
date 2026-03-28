using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TechVault.API.Health;

namespace TechVault.API.Controllers;

/// <summary>Liveness/readiness: database connectivity and process uptime (not under <c>/api/v{version}</c>).</summary>
[ApiController]
[ApiVersionNeutral]
[AllowAnonymous]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>Overall health, uptime, and per-database check results.</summary>
    [HttpGet]
    [Tags("Health")]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAsync(
        [FromServices] HealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);
        var uptimeSeconds = (DateTime.UtcNow - ApplicationUptime.StartedAtUtc).TotalSeconds;

        var checks = report.Entries.ToDictionary(
            static e => e.Key,
            static e => new HealthCheckEntryDto
            {
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                DurationMs = Math.Round(e.Value.Duration.TotalMilliseconds, 3)
            });

        var body = new HealthStatusResponse
        {
            Status = report.Status.ToString(),
            UptimeSeconds = Math.Round(uptimeSeconds, 3),
            Checks = checks
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(body)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}
