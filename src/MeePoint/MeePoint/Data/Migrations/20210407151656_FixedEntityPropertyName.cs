using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class FixedEntityPropertyName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusRestaurante",
                table: "Entities");

            migrationBuilder.AddColumn<bool>(
                name: "StatusEntity",
                table: "Entities",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusEntity",
                table: "Entities");

            migrationBuilder.AddColumn<bool>(
                name: "StatusRestaurante",
                table: "Entities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
