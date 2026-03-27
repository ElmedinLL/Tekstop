namespace TechVault.API.Errors;

/// <summary>Standard API error body for clients and logs.</summary>
public sealed class ApiErrorResponse
{
    public int StatusCode { get; init; }

    public string Message { get; init; } = "";

    /// <summary>Optional structured errors (e.g. field validation). Null when not applicable.</summary>
    public object? Errors { get; init; }
}
