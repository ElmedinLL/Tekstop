using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

/// <summary>Adds denormalized review stats on Products and backfills from Reviews.</summary>
public class ProductReviewAggregates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "AverageRating",
            table: "Products",
            type: "decimal(4,2)",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ReviewCount",
            table: "Products",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.Sql(
            """
            UPDATE Products p
            LEFT JOIN (
              SELECT ProductId,
                     ROUND(AVG(Rating), 2) AS AvgR,
                     COUNT(*) AS Cnt
              FROM Reviews
              WHERE IsApproved = 1
              GROUP BY ProductId
            ) r ON p.Id = r.ProductId
            SET p.AverageRating = r.AvgR,
                p.ReviewCount = COALESCE(r.Cnt, 0);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AverageRating",
            table: "Products");

        migrationBuilder.DropColumn(
            name: "ReviewCount",
            table: "Products");
    }
}
