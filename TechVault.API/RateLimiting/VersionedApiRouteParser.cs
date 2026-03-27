using System.Text.RegularExpressions;

namespace TechVault.API.RateLimiting;

internal static partial class VersionedApiRouteParser
{
    /// <summary>True for paths like <c>/api/v1/auth/...</c> or <c>/api/v2/auth</c>.</summary>
    public static bool IsVersionedAuthPath(PathString path)
    {
        var value = path.Value;
        return value is not null && VersionedAuthRegex().IsMatch(value);
    }

    /// <summary>True for <c>/api/v...</c> storefront/admin JSON routes (any version segment).</summary>
    public static bool IsVersionedApiPath(PathString path)
    {
        var value = path.Value;
        return value is not null && VersionedApiRegex().IsMatch(value);
    }

    [GeneratedRegex("^/api/v\\d+(?:\\.\\d+)?/auth(?:/|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionedAuthRegex();

    [GeneratedRegex("^/api/v\\d+(?:\\.\\d+)?/", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionedApiRegex();
}
