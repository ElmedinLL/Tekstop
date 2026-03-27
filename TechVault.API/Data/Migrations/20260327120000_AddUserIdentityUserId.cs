using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechVault.API.Data.Migrations;

public class AddUserIdentityUserId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "IdentityUserId",
            table: "Users",
            type: "varchar(450)",
            maxLength: 450,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Users_IdentityUserId",
            table: "Users",
            column: "IdentityUserId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_IdentityUserId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "IdentityUserId",
            table: "Users");
    }
}
