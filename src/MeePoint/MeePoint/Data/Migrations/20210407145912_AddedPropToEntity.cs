using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class AddedPropToEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MyProperty",
                table: "RegisteredUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Entities",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Entities",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManagerName",
                table: "Entities",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "Entities",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Entities",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Entities",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StatusRestaurante",
                table: "Entities",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyProperty",
                table: "RegisteredUsers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "ManagerName",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Entities");

            migrationBuilder.DropColumn(
                name: "StatusRestaurante",
                table: "Entities");
        }
    }
}
