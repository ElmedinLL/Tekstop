namespace TechVault.API.Health;

public sealed class HealthStatusResponse
{
    public required string Status { get; init; }

    /// <summary>Seconds since the API process recorded its start time.</summary>
    public double UptimeSeconds { get; init; }

    public required IReadOnlyDictionary<string, HealthCheckEntryDto> Checks { get; init; }
}

public sealed class HealthCheckEntryDto
{
    public required string Status { get; init; }

    public string? Description { get; init; }

    public double? DurationMs { get; init; }
}
