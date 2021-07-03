using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class DroppedUniqueConstraintOnEntityName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entities_Name",
                table: "Entities");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_Name",
                table: "Entities",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Entities_Name",
                table: "Entities");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_Name",
                table: "Entities",
                column: "Name",
                unique: true);
        }
    }
}
