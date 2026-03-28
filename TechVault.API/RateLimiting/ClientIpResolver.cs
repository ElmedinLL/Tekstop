namespace TechVault.API.RateLimiting;

internal static class ClientIpResolver
{
    private const string XForwardedForHeader = "X-Forwarded-For";

    public static string Resolve(HttpContext context)
    {
        var forwarded = context.Request.Headers[XForwardedForHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            var first = forwarded.Split(',')[0].Trim();
            if (first.Length > 0)
            {
                return first;
            }
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
