using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

public class OrderIdentityUserId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Orders_Users_UserId",
            table: "Orders");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Orders",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AddColumn<string>(
            name: "IdentityUserId",
            table: "Orders",
            type: "varchar(450)",
            maxLength: 450,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_IdentityUserId",
            table: "Orders",
            column: "IdentityUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Orders_IdentityUserId_Status",
            table: "Orders",
            columns: new[] { "IdentityUserId", "Status" });

        migrationBuilder.AddForeignKey(
            name: "FK_Orders_Users_UserId",
            table: "Orders",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Orders_Users_UserId",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "IX_Orders_IdentityUserId",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "IX_Orders_IdentityUserId_Status",
            table: "Orders");

        migrationBuilder.DropColumn(
            name: "IdentityUserId",
            table: "Orders");

        migrationBuilder.AlterColumn<int>(
            name: "UserId",
            table: "Orders",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Orders_Users_UserId",
            table: "Orders",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}
