using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TechVault.API.Auth;

namespace TechVault.API.Data;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseSqlServer(
            DesignTimeConnectionString.GetDefaultConnection(),
            sql => sql.MigrationsHistoryTable("__EFAuthMigrationsHistory"));

        return new AuthDbContext(optionsBuilder.Options);
    }
}