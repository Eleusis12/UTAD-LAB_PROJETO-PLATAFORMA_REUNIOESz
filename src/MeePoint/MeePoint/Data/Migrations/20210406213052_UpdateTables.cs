using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class UpdateTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convocations_RegisteredUsers_UserID",
                table: "Convocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Entities_RegisteredUsers_Manager",
                table: "Entities");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_RegisteredUsers_UserID",
                table: "GroupMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredUsers",
                table: "RegisteredUsers");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "RegisteredUsers");

            migrationBuilder.AddColumn<int>(
                name: "RegisteredUserID",
                table: "RegisteredUsers",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredUsers",
                table: "RegisteredUsers",
                column: "RegisteredUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Convocations_RegisteredUsers_UserID",
                table: "Convocations",
                column: "UserID",
                principalTable: "RegisteredUsers",
                principalColumn: "RegisteredUserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_RegisteredUsers_Manager",
                table: "Entities",
                column: "Manager",
                principalTable: "RegisteredUsers",
                principalColumn: "RegisteredUserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_RegisteredUsers_UserID",
                table: "GroupMembers",
                column: "UserID",
                principalTable: "RegisteredUsers",
                principalColumn: "RegisteredUserID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convocations_RegisteredUsers_UserID",
                table: "Convocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Entities_RegisteredUsers_Manager",
                table: "Entities");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_RegisteredUsers_UserID",
                table: "GroupMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RegisteredUsers",
                table: "RegisteredUsers");

            migrationBuilder.DropColumn(
                name: "RegisteredUserID",
                table: "RegisteredUsers");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "RegisteredUsers",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RegisteredUsers",
                table: "RegisteredUsers",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Convocations_RegisteredUsers_UserID",
                table: "Convocations",
                column: "UserID",
                principalTable: "RegisteredUsers",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entities_RegisteredUsers_Manager",
                table: "Entities",
                column: "Manager",
                principalTable: "RegisteredUsers",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_RegisteredUsers_UserID",
                table: "GroupMembers",
                column: "UserID",
                principalTable: "RegisteredUsers",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
