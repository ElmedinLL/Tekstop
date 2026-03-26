using System.Text;

namespace TechVault.API.Services;

internal static class ProductSlugHelper
{
    private const int MaxSlugLength = 280;

    /// <summary>Produces a URL-safe slug; falls back to <c>product</c> when nothing usable remains.</summary>
    public static string SlugifyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "product";
        }

        var sb = new StringBuilder();
        var pendingDash = false;

        foreach (var c in name.Trim().ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (pendingDash && sb.Length > 0)
                {
                    sb.Append('-');
                }

                pendingDash = false;
                sb.Append(c);
            }
            else if (char.IsWhiteSpace(c) || c is '-' or '_')
            {
                pendingDash = sb.Length > 0;
            }
        }

        var result = sb.ToString().Trim('-');
        if (result.Length > MaxSlugLength)
        {
            result = result[..MaxSlugLength].TrimEnd('-');
        }

        return string.IsNullOrEmpty(result) ? "product" : result;
    }

    public static string TruncateForSuffix(string baseSlug, string suffix)
    {
        if (baseSlug.Length + suffix.Length <= MaxSlugLength)
        {
            return baseSlug;
        }

        var max = MaxSlugLength - suffix.Length;
        return max <= 0 ? suffix.TrimStart('-') : baseSlug[..max].TrimEnd('-');
    }
}
