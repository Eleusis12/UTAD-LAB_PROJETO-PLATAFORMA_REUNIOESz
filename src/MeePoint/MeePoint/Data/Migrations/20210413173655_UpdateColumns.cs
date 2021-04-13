using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class UpdateColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegisteredUsers_Username",
                table: "RegisteredUsers");

            migrationBuilder.DropColumn(
                name: "MyProperty",
                table: "RegisteredUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "RegisteredUsers",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "RegisteredUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredUsers_Username",
                table: "RegisteredUsers",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegisteredUsers_Username",
                table: "RegisteredUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "RegisteredUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "RegisteredUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MyProperty",
                table: "RegisteredUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredUsers_Username",
                table: "RegisteredUsers",
                column: "Username",
                unique: true);
        }
    }
}
