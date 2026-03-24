using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechVault.API.Data;

/// <summary>
/// Used by EF Core tools (migrations, scaffolding) when no host is running.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        optionsBuilder.UseMySql(
            "Server=localhost;Port=3306;Database=TechVaultDB;User Id=root;Password=;",
            serverVersion);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
