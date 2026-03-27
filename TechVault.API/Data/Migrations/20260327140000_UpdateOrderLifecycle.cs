using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

public class UpdateOrderLifecycle : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE Orders SET Status = 5 WHERE Status = 6;");

        migrationBuilder.AddColumn<DateTime>(
            name: "CancelledAtUtc",
            table: "Orders",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "ConfirmedAtUtc",
            table: "Orders",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CouponCode",
            table: "Orders",
            type: "varchar(64)",
            maxLength: 64,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<decimal>(
            name: "DiscountAmount",
            table: "Orders",
            type: "decimal(18,2)",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<string>(
            name: "PaymentMethod",
            table: "Orders",
            type: "varchar(32)",
            maxLength: 32,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<DateTime>(
            name: "ProcessingAtUtc",
            table: "Orders",
            type: "datetime(6)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "ProcessingAtUtc", table: "Orders");
        migrationBuilder.DropColumn(name: "PaymentMethod", table: "Orders");
        migrationBuilder.DropColumn(name: "DiscountAmount", table: "Orders");
        migrationBuilder.DropColumn(name: "CouponCode", table: "Orders");
        migrationBuilder.DropColumn(name: "ConfirmedAtUtc", table: "Orders");
        migrationBuilder.DropColumn(name: "CancelledAtUtc", table: "Orders");
    }
}
