using Microsoft.EntityFrameworkCore;

namespace TechVault.API.Data;

public class TechVaultDbContext : DbContext
{
    public TechVaultDbContext(DbContextOptions<TechVaultDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
