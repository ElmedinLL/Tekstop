using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;
using Xunit;

namespace TechVault.API.Tests;

/// <summary>
/// Integration test host. By default starts SQL Server in Docker (Testcontainers) so tests pass on Linux CI.
/// For local runs without Docker, set environment variable <c>TECHVAULT_TEST_CONNECTION_STRING</c> to a SQL Server
/// connection string (e.g. LocalDB: same format as previously hard-coded in this factory).
/// </summary>
public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string LocalDbConnectionStringExample =
        "Server=(localdb)\\mssqllocaldb;Database=TechVaultDB_IntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    private const string IntegrationDatabaseName = "TechVaultDB_IntegrationTests";

    private MsSqlContainer? _sql;
    private string? _resolvedConnectionString;

    /// <summary>Seeded on host startup for admin-route tests (see TechVault.API.Auth.IdentitySeeder).</summary>
    public const string IntegrationAdminEmail = "integration-admin@techvault.test";

    public const string IntegrationAdminPassword = "AdminPass1!";

    public async Task InitializeAsync()
    {
        var envOverride = Environment.GetEnvironmentVariable("TECHVAULT_TEST_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(envOverride))
        {
            _resolvedConnectionString = envOverride.Trim();
            return;
        }

        _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("IntegrationTests1!")
            .Build();
        await _sql.StartAsync();

        var csb = new SqlConnectionStringBuilder(_sql.GetConnectionString())
        {
            InitialCatalog = IntegrationDatabaseName,
            MultipleActiveResultSets = true,
            TrustServerCertificate = true
        };

        _resolvedConnectionString = csb.ConnectionString;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        if (_sql is not null)
        {
            await _sql.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (string.IsNullOrEmpty(_resolvedConnectionString))
        {
            throw new InvalidOperationException(
                "Integration tests need SQL Server. Either start Docker (Testcontainers) or set environment variable " +
                "TECHVAULT_TEST_CONNECTION_STRING to a SQL Server connection string (see LocalDbConnectionStringExample on ApiWebApplicationFactory).");
        }

        var connectionString = _resolvedConnectionString;

        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["IdentitySeed:AdminEmail"] = IntegrationAdminEmail,
                ["IdentitySeed:AdminPassword"] = IntegrationAdminPassword,
                ["IdentitySeed:SyncAdminPassword"] = "true",
            });
        });
    }
}
