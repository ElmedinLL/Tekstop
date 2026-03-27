using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

public class AddCoupons : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Coupons",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                DiscountType = table.Column<int>(type: "int", nullable: false),
                DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                MinOrderValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Coupons", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Coupons_Code",
            table: "Coupons",
            column: "Code",
            unique: true);

        migrationBuilder.InsertData(
            table: "Coupons",
            columns: new[] { "Id", "Code", "DiscountType", "DiscountValue", "MinOrderValue", "ExpiresAtUtc", "IsActive" },
            values: new object[] { 1, "WELCOME10", 0, 10.00m, 50.00m, new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc), true });

        migrationBuilder.InsertData(
            table: "Coupons",
            columns: new[] { "Id", "Code", "DiscountType", "DiscountValue", "MinOrderValue", "ExpiresAtUtc", "IsActive" },
            values: new object[] { 2, "SAVE25", 1, 25.00m, 100.00m, new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc), true });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Coupons");
    }
}
