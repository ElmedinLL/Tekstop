namespace TechVault.API.RateLimiting;

internal static class ClientIpResolver
{
    public static string Resolve(HttpContext context)
    {
        var forwarded = context.Request.Headers.ForwardedFor.FirstOrDefault();
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
