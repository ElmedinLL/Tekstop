using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

/// <summary>Adds InnoDB FULLTEXT index on <c>Products.Name</c> and <c>Products.Description</c> for MySQL <c>MATCH ... AGAINST</c>.</summary>
public partial class AddProductFullTextIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE `Products` ADD FULLTEXT INDEX `IX_Products_Name_Description` (`Name`, `Description`);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE `Products` DROP INDEX `IX_Products_Name_Description`;");
    }
}
