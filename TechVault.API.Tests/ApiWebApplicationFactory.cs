using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TechVault.API.Tests;

/// <summary>Uses a dedicated LocalDB database so tests do not touch your dev TechVaultDB.</summary>
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string IntegrationConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=TechVaultDB_IntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    /// <summary>Seeded on host startup for admin-route tests (see <see cref="IdentitySeeder"/>).</summary>
    public const string IntegrationAdminEmail = "integration-admin@techvault.test";

    public const string IntegrationAdminPassword = "AdminPass1!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", IntegrationConnectionString);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = IntegrationConnectionString,
                ["IdentitySeed:AdminEmail"] = IntegrationAdminEmail,
                ["IdentitySeed:AdminPassword"] = IntegrationAdminPassword,
                ["IdentitySeed:SyncAdminPassword"] = "true",
            });
        });
    }
}
