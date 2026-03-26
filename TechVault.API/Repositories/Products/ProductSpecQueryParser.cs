using System.Text.RegularExpressions;

namespace TechVault.API.Repositories.Products;

/// <summary>Parses repeated query values like <c>RAM:16GB</c> into spec key/value pairs for JSON filtering.</summary>
public static class ProductSpecQueryParser
{
    private static readonly Regex ValidKey = new(@"^[a-zA-Z][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    /// <summary>
    /// Each entry is <c>Key:Value</c> (first colon separates key from value; value may contain colons).
    /// Keys must match <c>[a-zA-Z][a-zA-Z0-9_]*</c> for safe JSON paths.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>>? Parse(IEnumerable<string>? raw)
    {
        if (raw is null)
        {
            return null;
        }

        var list = new List<KeyValuePair<string, string>>();
        foreach (var s in raw)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                continue;
            }

            var idx = s.IndexOf(':');
            if (idx <= 0 || idx >= s.Length - 1)
            {
                continue;
            }

            var key = s[..idx].Trim();
            var val = s[(idx + 1)..].Trim();
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(val) || !ValidKey.IsMatch(key))
            {
                continue;
            }

            list.Add(new KeyValuePair<string, string>(key, val));
        }

        return list.Count == 0 ? null : list;
    }
}
