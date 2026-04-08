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
        optionsBuilder.UseSqlServer(DesignTimeConnectionString.GetDefaultConnection());

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
