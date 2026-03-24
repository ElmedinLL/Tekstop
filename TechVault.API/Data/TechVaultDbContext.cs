using Microsoft.EntityFrameworkCore;
using TechVault.API.Models;

namespace TechVault.API.Data;

public class TechVaultDbContext : DbContext
{
    public TechVaultDbContext(DbContextOptions<TechVaultDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PasswordHash).HasMaxLength(512);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(32);
            entity.Property(e => e.Role).HasConversion<int>();
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.Property(e => e.Label).HasMaxLength(64);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Line1).HasMaxLength(256);
            entity.Property(e => e.Line2).HasMaxLength(256);
            entity.Property(e => e.City).HasMaxLength(128);
            entity.Property(e => e.Region).HasMaxLength(128);
            entity.Property(e => e.PostalCode).HasMaxLength(32);
            entity.Property(e => e.Country).HasMaxLength(128);
            entity.Property(e => e.Phone).HasMaxLength(32);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(160);
            entity.Property(e => e.Slug).HasMaxLength(180);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.ParentCategory)
                .WithMany(e => e.ChildCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.Slug).HasMaxLength(280);
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.Sku).HasMaxLength(64);
            entity.Property(e => e.Brand).HasMaxLength(120);
            entity.Property(e => e.ImageUrl).HasMaxLength(2048);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.CompareAtPrice).HasPrecision(18, 2);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.OrderNumber).HasMaxLength(32);
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(8);
            entity.Property(e => e.ShippingFullName).HasMaxLength(200);
            entity.Property(e => e.ShippingLine1).HasMaxLength(256);
            entity.Property(e => e.ShippingLine2).HasMaxLength(256);
            entity.Property(e => e.ShippingCity).HasMaxLength(128);
            entity.Property(e => e.ShippingRegion).HasMaxLength(128);
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(32);
            entity.Property(e => e.ShippingCountry).HasMaxLength(128);
            entity.Property(e => e.ShippingPhone).HasMaxLength(32);
            entity.Property(e => e.BillingFullName).HasMaxLength(200);
            entity.Property(e => e.BillingLine1).HasMaxLength(256);
            entity.Property(e => e.BillingLine2).HasMaxLength(256);
            entity.Property(e => e.BillingCity).HasMaxLength(128);
            entity.Property(e => e.BillingRegion).HasMaxLength(128);
            entity.Property(e => e.BillingPostalCode).HasMaxLength(32);
            entity.Property(e => e.BillingCountry).HasMaxLength(128);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ShippingAddress)
                .WithMany()
                .HasForeignKey(e => e.ShippingAddressId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(e => e.ProductName).HasMaxLength(256);
            entity.Property(e => e.ProductSku).HasMaxLength(64);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Comment).HasMaxLength(4000);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
