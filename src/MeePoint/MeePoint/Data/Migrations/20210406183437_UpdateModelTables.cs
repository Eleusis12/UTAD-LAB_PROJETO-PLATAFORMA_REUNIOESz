using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class UpdateModelTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Entities_EntityID",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "EntityNIF",
                table: "Groups");

            migrationBuilder.AlterColumn<int>(
                name: "EntityID",
                table: "Groups",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Entities_EntityID",
                table: "Groups",
                column: "EntityID",
                principalTable: "Entities",
                principalColumn: "EntityID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Entities_EntityID",
                table: "Groups");

            migrationBuilder.AlterColumn<int>(
                name: "EntityID",
                table: "Groups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "EntityNIF",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Entities_EntityID",
                table: "Groups",
                column: "EntityID",
                principalTable: "Entities",
                principalColumn: "EntityID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
