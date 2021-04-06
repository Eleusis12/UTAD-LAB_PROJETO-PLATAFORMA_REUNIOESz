using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class UpdateUniquenessAndKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "RegisteredUsers",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "RegisteredUsers",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RegisteredUsers",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Groups",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "GroupMembers",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Entities",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Manager",
                table: "Entities",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentPath",
                table: "Documents",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredUsers_Email",
                table: "RegisteredUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredUsers_PasswordHash",
                table: "RegisteredUsers",
                column: "PasswordHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredUsers_Username",
                table: "RegisteredUsers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Name",
                table: "Groups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entities_Manager",
                table: "Entities",
                column: "Manager");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_NIF",
                table: "Entities",
                column: "NIF",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entities_Name",
                table: "Entities",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_RegisteredUsers_Manager",
                table: "Entities",
                column: "Manager",
                principalTable: "RegisteredUsers",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entities_RegisteredUsers_Manager",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredUsers_Email",
                table: "RegisteredUsers");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredUsers_PasswordHash",
                table: "RegisteredUsers");

            migrationBuilder.DropIndex(
                name: "IX_RegisteredUsers_Username",
                table: "RegisteredUsers");

            migrationBuilder.DropIndex(
                name: "IX_Groups_Name",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Entities_Manager",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_Entities_NIF",
                table: "Entities");

            migrationBuilder.DropIndex(
                name: "IX_Entities_Name",
                table: "Entities");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "RegisteredUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "RegisteredUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RegisteredUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Groups",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "GroupMembers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Entities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Manager",
                table: "Entities",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentPath",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
