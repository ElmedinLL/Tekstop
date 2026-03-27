namespace TechVault.API.Health;

/// <summary>Process uptime anchor (set once when the host is built).</summary>
public static class ApplicationUptime
{
    private static DateTime? _startedAtUtc;

    public static DateTime StartedAtUtc => _startedAtUtc ?? throw new InvalidOperationException("Application start time was not recorded.");

    public static void MarkStarted() => _startedAtUtc ??= DateTime.UtcNow;
}
