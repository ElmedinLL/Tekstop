using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TechVault.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinOrderValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(280)", maxLength: 280, nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sku = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CompareAtPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    ImagesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AverageRating = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    ReviewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Line1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Line2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsDefaultShipping = table.Column<bool>(type: "bit", nullable: false),
                    IsDefaultBilling = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTags",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTags", x => new { x.ProductId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ProductTags_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Reviews_Rating", "[Rating] >= 1 AND [Rating] <= 5");
                    table.ForeignKey(
                        name: "FK_Reviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TrackingUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: true),
                    ShippingFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShippingLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ShippingLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShippingCity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShippingRegion = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ShippingCountry = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShippingPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    BillingFullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BillingLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BillingLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    BillingCity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BillingRegion = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BillingPostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BillingCountry = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PlacedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ShippedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Addresses_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ProductSku = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ExternalPaymentId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExternalChargeId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AmountCents = table.Column<long>(type: "bigint", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Description", "DisplayOrder", "ImageUrl", "IsActive", "Name", "ParentCategoryId", "Slug" },
                values: new object[,]
                {
                    { 1, "Notebooks and ultrabooks", 1, null, true, "Laptops", null, "laptops" },
                    { 2, "CPUs, GPUs, memory, storage", 2, null, true, "PC Components", null, "pc-components" },
                    { 3, "Keyboards, mice, audio", 3, null, true, "Peripherals", null, "peripherals" },
                    { 4, "Displays and panels", 4, null, true, "Monitors", null, "monitors" },
                    { 5, "Routers, switches, cables", 5, null, true, "Networking", null, "networking" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "Gaming", "gaming" },
                    { 2, "Professional", "professional" },
                    { 3, "Wireless", "wireless" },
                    { 4, "RGB", "rgb" },
                    { 5, "Budget", "budget" },
                    { 6, "Pro", "pro" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AverageRating", "Brand", "CategoryId", "CompareAtPrice", "CreatedAtUtc", "DeletedAtUtc", "Description", "ImageUrl", "ImagesJson", "IsPublished", "Name", "Price", "ShortDescription", "Sku", "Slug", "SpecsJson", "StockQuantity", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, null, "TechVault", 1, 1099.99m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/probook-14.jpg", null, true, "TechVault ProBook 14", 999.99m, "14\" business ultrabook", "TV-LAP-001", "techvault-probook-14", null, 40, null },
                    { 2, null, "TechVault", 1, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/blade-16.jpg", null, true, "TechVault Blade 16", 1899.00m, "16\" creator laptop", "TV-LAP-002", "techvault-blade-16", null, 15, null },
                    { 3, null, "TechVault", 1, 849.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/air-13.jpg", null, true, "TechVault Air 13", 799.00m, "13\" lightweight daily driver", "TV-LAP-003", "techvault-air-13", null, 60, null },
                    { 4, null, "TechVault", 1, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/station-17.jpg", null, true, "TechVault Station 17", 2499.00m, "17\" workstation", "TV-LAP-004", "techvault-station-17", null, 10, null },
                    { 5, null, "NovaGraph", 2, 749.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/gpu-780.jpg", null, true, "NovaGraph RTX 780", 699.00m, "High-end graphics card", "TV-GPU-001", "novagraph-rtx-780", null, 25, null },
                    { 6, null, "CorePeak", 2, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/cpu-i9.jpg", null, true, "CorePeak i9-14900K", 549.00m, "Desktop CPU unlocked", "TV-CPU-001", "corepeak-i9-14900k", null, 30, null },
                    { 7, null, "RAMBurst", 2, 149.99m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/ram-32gb.jpg", null, true, "RAMBurst DDR5 32GB Kit", 129.99m, "2x16GB DDR5-6000", "TV-RAM-001", "ramburst-ddr5-32gb", null, 100, null },
                    { 8, null, "FlashForge", 2, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/ssd-2tb.jpg", null, true, "FlashForge NVMe 2TB", 179.00m, "PCIe Gen4 SSD", "TV-SSD-001", "flashforge-nvme-2tb", null, 55, null },
                    { 9, null, "KeyForge", 3, 139.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/kb-rgb.jpg", null, true, "KeyForge Mechanical RGB", 119.00m, "TKL mechanical keyboard", "TV-KB-001", "keyforge-mechanical-rgb", null, 80, null },
                    { 10, null, "GlideAir", 3, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/mouse-wl.jpg", null, true, "GlideAir Wireless Mouse", 59.99m, "Ergonomic wireless mouse", "TV-MS-001", "glideair-wireless-mouse", null, 120, null },
                    { 11, null, "SoundArc", 3, 99.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/headset.jpg", null, true, "SoundArc 7.1 Headset", 89.00m, "USB gaming headset", "TV-HS-001", "soundarc-71-headset", null, 70, null },
                    { 12, null, "ClearView", 3, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/webcam.jpg", null, true, "ClearView 4K Webcam", 129.00m, "Auto-focus conference cam", "TV-WC-001", "clearview-4k-webcam", null, 45, null },
                    { 13, null, "PixelPro", 4, 229.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/mon-24.jpg", null, true, "PixelPro 24\" FHD", 199.00m, "1080p 144Hz IPS", "TV-MON-001", "pixelpro-24-fhd", null, 50, null },
                    { 14, null, "PixelPro", 4, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/mon-27.jpg", null, true, "PixelPro 27\" QHD", 349.00m, "1440p 165Hz IPS", "TV-MON-002", "pixelpro-27-qhd", null, 35, null },
                    { 15, null, "PixelPro", 4, 649.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/mon-32.jpg", null, true, "PixelPro 32\" 4K", 599.00m, "4K HDR creator panel", "TV-MON-003", "pixelpro-32-4k", null, 20, null },
                    { 16, null, "PixelPro", 4, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/mon-portable.jpg", null, true, "TravelPanel 15\" USB-C", 249.00m, "Portable USB-C monitor", "TV-MON-004", "travelpanel-15-usbc", null, 40, null },
                    { 17, null, "LinkHub", 5, 299.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/router.jpg", null, true, "LinkHub AX6000 Router", 279.00m, "Wi-Fi 6E router", "TV-NET-001", "linkhub-ax6000-router", null, 30, null },
                    { 18, null, "LinkHub", 5, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/switch.jpg", null, true, "LinkHub 8-Port Switch", 39.99m, "Gigabit unmanaged switch", "TV-NET-002", "linkhub-8port-switch", null, 90, null },
                    { 19, null, "LinkHub", 5, 359.00m, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/mesh.jpg", null, true, "LinkHub Mesh Trio", 329.00m, "Whole-home mesh Wi-Fi", "TV-NET-003", "linkhub-mesh-trio", null, 22, null },
                    { 20, null, "CableCraft", 5, null, new DateTime(2024, 6, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "/images/products/cables.jpg", null, true, "CableCraft Cat6 Kit", 24.99m, "25ft patch cables (5-pack)", "TV-NET-004", "cablecraft-cat6-kit", null, 200, null }
                });

            migrationBuilder.InsertData(
                table: "ProductTags",
                columns: new[] { "ProductId", "TagId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 1, 6 },
                    { 2, 1 },
                    { 2, 6 },
                    { 3, 3 },
                    { 3, 5 },
                    { 4, 2 },
                    { 4, 6 },
                    { 5, 1 },
                    { 5, 4 },
                    { 6, 2 },
                    { 6, 6 },
                    { 7, 5 },
                    { 7, 6 },
                    { 8, 5 },
                    { 8, 6 },
                    { 9, 1 },
                    { 9, 4 },
                    { 10, 3 },
                    { 10, 5 },
                    { 11, 1 },
                    { 11, 4 },
                    { 12, 2 },
                    { 12, 3 },
                    { 13, 1 },
                    { 13, 5 },
                    { 14, 1 },
                    { 14, 6 },
                    { 15, 2 },
                    { 15, 6 },
                    { 16, 2 },
                    { 16, 3 },
                    { 17, 3 },
                    { 17, 6 },
                    { 18, 5 },
                    { 18, 6 },
                    { 19, 3 },
                    { 19, 6 },
                    { 20, 5 },
                    { 20, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_IdentityUserId_ProductId",
                table: "CartItems",
                columns: new[] { "IdentityUserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IdentityUserId",
                table: "Orders",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_IdentityUserId_Status",
                table: "Orders",
                columns: new[] { "IdentityUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PlacedAtUtc",
                table: "Orders",
                column: "PlacedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_Status",
                table: "Orders",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalPaymentId",
                table: "Payments",
                column: "ExternalPaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Brand",
                table: "Products",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsPublished",
                table: "Products",
                columns: new[] { "CategoryId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsDeleted",
                table: "Products",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTags_TagId",
                table: "ProductTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_ProductId",
                table: "Reviews",
                columns: new[] { "UserId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Slug",
                table: "Tags",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_IdentityUserId",
                table: "Users",
                column: "IdentityUserId",
                unique: true,
                filter: "[IdentityUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_ProductId",
                table: "WishlistItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_UserId_ProductId",
                table: "WishlistItems",
                columns: new[] { "UserId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductTags");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
