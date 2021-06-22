using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
	public partial class RemoveUniqueConstraintGroups : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Groups_Name",
				table: "Groups");

			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "Groups",
				nullable: false,
				oldClrType: typeof(string),
				oldType: "nvarchar(450)");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Name",
				table: "Groups",
				type: "nvarchar(450)",
				nullable: false,
				oldClrType: typeof(string));

			migrationBuilder.CreateIndex(
				name: "IX_Groups_Name",
				table: "Groups",
				column: "Name",
				unique: true);
		}
	}
}