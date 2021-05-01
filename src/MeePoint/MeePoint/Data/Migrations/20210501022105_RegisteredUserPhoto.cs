using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class RegisteredUserPhoto : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Foto",
                table: "RegisteredUsers",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Foto",
                table: "RegisteredUsers");
        }
    }
}
