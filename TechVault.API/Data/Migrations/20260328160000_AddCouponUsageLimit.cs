using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

public class AddCouponUsageLimit : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "UsageLimit",
            table: "Coupons",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "UsageCount",
            table: "Coupons",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UsageLimit",
            table: "Coupons");

        migrationBuilder.DropColumn(
            name: "UsageCount",
            table: "Coupons");
    }
}
