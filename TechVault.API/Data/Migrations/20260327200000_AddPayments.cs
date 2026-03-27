using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

public class AddPayments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                OrderId = table.Column<int>(type: "int", nullable: false),
                Provider = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ExternalPaymentId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ExternalChargeId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                AmountCents = table.Column<long>(type: "bigint", nullable: false),
                Currency = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_OrderId",
            table: "Payments",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_Payments_ExternalPaymentId",
            table: "Payments",
            column: "ExternalPaymentId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Payments");
    }
}
