using Microsoft.EntityFrameworkCore;
using TechVault.API.Models;

namespace TechVault.API.Data;

internal static class ApplicationDbContextSeed
{
    private static readonly DateTime SeedUtc = new(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedCategories(modelBuilder);
        SeedTags(modelBuilder);
        SeedProducts(modelBuilder);
        SeedProductTags(modelBuilder);
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Laptops", Slug = "laptops", Description = "Notebooks and ultrabooks", ParentCategoryId = null, DisplayOrder = 1, IsActive = true },
            new Category { Id = 2, Name = "PC Components", Slug = "pc-components", Description = "CPUs, GPUs, memory, storage", ParentCategoryId = null, DisplayOrder = 2, IsActive = true },
            new Category { Id = 3, Name = "Peripherals", Slug = "peripherals", Description = "Keyboards, mice, audio", ParentCategoryId = null, DisplayOrder = 3, IsActive = true },
            new Category { Id = 4, Name = "Monitors", Slug = "monitors", Description = "Displays and panels", ParentCategoryId = null, DisplayOrder = 4, IsActive = true },
            new Category { Id = 5, Name = "Networking", Slug = "networking", Description = "Routers, switches, cables", ParentCategoryId = null, DisplayOrder = 5, IsActive = true });
    }

    private static void SeedTags(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>().HasData(
            new Tag { Id = 1, Name = "Gaming", Slug = "gaming" },
            new Tag { Id = 2, Name = "Professional", Slug = "professional" },
            new Tag { Id = 3, Name = "Wireless", Slug = "wireless" },
            new Tag { Id = 4, Name = "RGB", Slug = "rgb" },
            new Tag { Id = 5, Name = "Budget", Slug = "budget" },
            new Tag { Id = 6, Name = "Pro", Slug = "pro" });
    }

    private static void SeedProducts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, CategoryId = 1, Name = "TechVault ProBook 14", Slug = "techvault-probook-14", ShortDescription = "14\" business ultrabook", Sku = "TV-LAP-001", Price = 999.99m, CompareAtPrice = 1099.99m, StockQuantity = 40, Brand = "TechVault", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/probook-14.jpg" },
            new Product { Id = 2, CategoryId = 1, Name = "TechVault Blade 16", Slug = "techvault-blade-16", ShortDescription = "16\" creator laptop", Sku = "TV-LAP-002", Price = 1899.00m, CompareAtPrice = null, StockQuantity = 15, Brand = "TechVault", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/blade-16.jpg" },
            new Product { Id = 3, CategoryId = 1, Name = "TechVault Air 13", Slug = "techvault-air-13", ShortDescription = "13\" lightweight daily driver", Sku = "TV-LAP-003", Price = 799.00m, CompareAtPrice = 849.00m, StockQuantity = 60, Brand = "TechVault", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/air-13.jpg" },
            new Product { Id = 4, CategoryId = 1, Name = "TechVault Station 17", Slug = "techvault-station-17", ShortDescription = "17\" workstation", Sku = "TV-LAP-004", Price = 2499.00m, CompareAtPrice = null, StockQuantity = 10, Brand = "TechVault", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/station-17.jpg" },
            new Product { Id = 5, CategoryId = 2, Name = "NovaGraph RTX 780", Slug = "novagraph-rtx-780", ShortDescription = "High-end graphics card", Sku = "TV-GPU-001", Price = 699.00m, CompareAtPrice = 749.00m, StockQuantity = 25, Brand = "NovaGraph", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/gpu-780.jpg" },
            new Product { Id = 6, CategoryId = 2, Name = "CorePeak i9-14900K", Slug = "corepeak-i9-14900k", ShortDescription = "Desktop CPU unlocked", Sku = "TV-CPU-001", Price = 549.00m, CompareAtPrice = null, StockQuantity = 30, Brand = "CorePeak", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/cpu-i9.jpg" },
            new Product { Id = 7, CategoryId = 2, Name = "RAMBurst DDR5 32GB Kit", Slug = "ramburst-ddr5-32gb", ShortDescription = "2x16GB DDR5-6000", Sku = "TV-RAM-001", Price = 129.99m, CompareAtPrice = 149.99m, StockQuantity = 100, Brand = "RAMBurst", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/ram-32gb.jpg" },
            new Product { Id = 8, CategoryId = 2, Name = "FlashForge NVMe 2TB", Slug = "flashforge-nvme-2tb", ShortDescription = "PCIe Gen4 SSD", Sku = "TV-SSD-001", Price = 179.00m, CompareAtPrice = null, StockQuantity = 55, Brand = "FlashForge", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/ssd-2tb.jpg" },
            new Product { Id = 9, CategoryId = 3, Name = "KeyForge Mechanical RGB", Slug = "keyforge-mechanical-rgb", ShortDescription = "TKL mechanical keyboard", Sku = "TV-KB-001", Price = 119.00m, CompareAtPrice = 139.00m, StockQuantity = 80, Brand = "KeyForge", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/kb-rgb.jpg" },
            new Product { Id = 10, CategoryId = 3, Name = "GlideAir Wireless Mouse", Slug = "glideair-wireless-mouse", ShortDescription = "Ergonomic wireless mouse", Sku = "TV-MS-001", Price = 59.99m, CompareAtPrice = null, StockQuantity = 120, Brand = "GlideAir", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/mouse-wl.jpg" },
            new Product { Id = 11, CategoryId = 3, Name = "SoundArc 7.1 Headset", Slug = "soundarc-71-headset", ShortDescription = "USB gaming headset", Sku = "TV-HS-001", Price = 89.00m, CompareAtPrice = 99.00m, StockQuantity = 70, Brand = "SoundArc", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/headset.jpg" },
            new Product { Id = 12, CategoryId = 3, Name = "ClearView 4K Webcam", Slug = "clearview-4k-webcam", ShortDescription = "Auto-focus conference cam", Sku = "TV-WC-001", Price = 129.00m, CompareAtPrice = null, StockQuantity = 45, Brand = "ClearView", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/webcam.jpg" },
            new Product { Id = 13, CategoryId = 4, Name = "PixelPro 24\" FHD", Slug = "pixelpro-24-fhd", ShortDescription = "1080p 144Hz IPS", Sku = "TV-MON-001", Price = 199.00m, CompareAtPrice = 229.00m, StockQuantity = 50, Brand = "PixelPro", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/mon-24.jpg" },
            new Product { Id = 14, CategoryId = 4, Name = "PixelPro 27\" QHD", Slug = "pixelpro-27-qhd", ShortDescription = "1440p 165Hz IPS", Sku = "TV-MON-002", Price = 349.00m, CompareAtPrice = null, StockQuantity = 35, Brand = "PixelPro", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/mon-27.jpg" },
            new Product { Id = 15, CategoryId = 4, Name = "PixelPro 32\" 4K", Slug = "pixelpro-32-4k", ShortDescription = "4K HDR creator panel", Sku = "TV-MON-003", Price = 599.00m, CompareAtPrice = 649.00m, StockQuantity = 20, Brand = "PixelPro", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/mon-32.jpg" },
            new Product { Id = 16, CategoryId = 4, Name = "TravelPanel 15\" USB-C", Slug = "travelpanel-15-usbc", ShortDescription = "Portable USB-C monitor", Sku = "TV-MON-004", Price = 249.00m, CompareAtPrice = null, StockQuantity = 40, Brand = "PixelPro", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/mon-portable.jpg" },
            new Product { Id = 17, CategoryId = 5, Name = "LinkHub AX6000 Router", Slug = "linkhub-ax6000-router", ShortDescription = "Wi-Fi 6E router", Sku = "TV-NET-001", Price = 279.00m, CompareAtPrice = 299.00m, StockQuantity = 30, Brand = "LinkHub", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/router.jpg" },
            new Product { Id = 18, CategoryId = 5, Name = "LinkHub 8-Port Switch", Slug = "linkhub-8port-switch", ShortDescription = "Gigabit unmanaged switch", Sku = "TV-NET-002", Price = 39.99m, CompareAtPrice = null, StockQuantity = 90, Brand = "LinkHub", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/switch.jpg" },
            new Product { Id = 19, CategoryId = 5, Name = "LinkHub Mesh Trio", Slug = "linkhub-mesh-trio", ShortDescription = "Whole-home mesh Wi-Fi", Sku = "TV-NET-003", Price = 329.00m, CompareAtPrice = 359.00m, StockQuantity = 22, Brand = "LinkHub", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/mesh.jpg" },
            new Product { Id = 20, CategoryId = 5, Name = "CableCraft Cat6 Kit", Slug = "cablecraft-cat6-kit", ShortDescription = "25ft patch cables (5-pack)", Sku = "TV-NET-004", Price = 24.99m, CompareAtPrice = null, StockQuantity = 200, Brand = "CableCraft", IsPublished = true, CreatedAtUtc = SeedUtc, ImageUrl = "/images/products/cables.jpg" });
    }

    private static void SeedProductTags(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductTag>().HasData(
            new ProductTag { ProductId = 1, TagId = 2 }, new ProductTag { ProductId = 1, TagId = 6 },
            new ProductTag { ProductId = 2, TagId = 1 }, new ProductTag { ProductId = 2, TagId = 6 },
            new ProductTag { ProductId = 3, TagId = 5 }, new ProductTag { ProductId = 3, TagId = 3 },
            new ProductTag { ProductId = 4, TagId = 2 }, new ProductTag { ProductId = 4, TagId = 6 },
            new ProductTag { ProductId = 5, TagId = 1 }, new ProductTag { ProductId = 5, TagId = 4 },
            new ProductTag { ProductId = 6, TagId = 2 }, new ProductTag { ProductId = 6, TagId = 6 },
            new ProductTag { ProductId = 7, TagId = 5 }, new ProductTag { ProductId = 7, TagId = 6 },
            new ProductTag { ProductId = 8, TagId = 5 }, new ProductTag { ProductId = 8, TagId = 6 },
            new ProductTag { ProductId = 9, TagId = 1 }, new ProductTag { ProductId = 9, TagId = 4 },
            new ProductTag { ProductId = 10, TagId = 3 }, new ProductTag { ProductId = 10, TagId = 5 },
            new ProductTag { ProductId = 11, TagId = 1 }, new ProductTag { ProductId = 11, TagId = 4 },
            new ProductTag { ProductId = 12, TagId = 2 }, new ProductTag { ProductId = 12, TagId = 3 },
            new ProductTag { ProductId = 13, TagId = 1 }, new ProductTag { ProductId = 13, TagId = 5 },
            new ProductTag { ProductId = 14, TagId = 1 }, new ProductTag { ProductId = 14, TagId = 6 },
            new ProductTag { ProductId = 15, TagId = 2 }, new ProductTag { ProductId = 15, TagId = 6 },
            new ProductTag { ProductId = 16, TagId = 3 }, new ProductTag { ProductId = 16, TagId = 2 },
            new ProductTag { ProductId = 17, TagId = 3 }, new ProductTag { ProductId = 17, TagId = 6 },
            new ProductTag { ProductId = 18, TagId = 5 }, new ProductTag { ProductId = 18, TagId = 6 },
            new ProductTag { ProductId = 19, TagId = 3 }, new ProductTag { ProductId = 19, TagId = 6 },
            new ProductTag { ProductId = 20, TagId = 5 }, new ProductTag { ProductId = 20, TagId = 6 });
    }
}
