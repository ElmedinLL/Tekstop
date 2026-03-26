using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

/// <summary>Adds soft-delete columns and index on <c>Products</c>.</summary>
public partial class AddProductSoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAtUtc",
            table: "Products",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Products",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_Products_IsDeleted",
            table: "Products",
            column: "IsDeleted");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Products_IsDeleted",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "DeletedAtUtc",
            table: "Products");
    }
}
