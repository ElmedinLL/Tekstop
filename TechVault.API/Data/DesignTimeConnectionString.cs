using Microsoft.Extensions.Configuration;

namespace TechVault.API.Data;

/// <summary>
/// Loads the same connection string as runtime (<c>appsettings.json</c> + optional Development + env)
/// so <c>dotnet ef</c> does not use a hard-coded empty password.
/// </summary>
internal static class DesignTimeConnectionString
{
    public static string GetDefaultConnection()
    {
        var basePath = Directory.GetCurrentDirectory();
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        return config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set it in appsettings.json.");
    }
}
