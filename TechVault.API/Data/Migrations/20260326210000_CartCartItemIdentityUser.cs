using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

public class CartCartItemIdentityUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM CartItems;");

        migrationBuilder.DropForeignKey(
            name: "FK_CartItems_Users_UserId",
            table: "CartItems");

        migrationBuilder.DropIndex(
            name: "IX_CartItems_UserId_ProductId",
            table: "CartItems");

        migrationBuilder.DropColumn(
            name: "UserId",
            table: "CartItems");

        migrationBuilder.AddColumn<string>(
            name: "IdentityUserId",
            table: "CartItems",
            type: "varchar(450)",
            maxLength: 450,
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_IdentityUserId_ProductId",
            table: "CartItems",
            columns: new[] { "IdentityUserId", "ProductId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CartItems_IdentityUserId_ProductId",
            table: "CartItems");

        migrationBuilder.DropColumn(
            name: "IdentityUserId",
            table: "CartItems");

        migrationBuilder.AddColumn<int>(
            name: "UserId",
            table: "CartItems",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_CartItems_UserId_ProductId",
            table: "CartItems",
            columns: new[] { "UserId", "ProductId" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_CartItems_Users_UserId",
            table: "CartItems",
            column: "UserId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
