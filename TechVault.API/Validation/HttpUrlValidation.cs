namespace TechVault.API.Validation;

internal static class HttpUrlValidation
{
    public static bool IsNullOrValidHttpOrHttpsUrl(string? url) =>
        string.IsNullOrWhiteSpace(url) || IsValidHttpOrHttpsUrl(url);

    public static bool IsValidHttpOrHttpsUrl(string url)
    {
        var trimmed = url.Trim();
        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
    }
}
