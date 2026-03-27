using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

/// <summary>Enforces Review.Rating between 1 and 5 at the database level.</summary>
public class ReviewRatingCheckConstraint : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE `Reviews` ADD CONSTRAINT `CK_Reviews_Rating` CHECK (`Rating` >= 1 AND `Rating` <= 5);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE `Reviews` DROP CHECK `CK_Reviews_Rating`;");
    }
}
