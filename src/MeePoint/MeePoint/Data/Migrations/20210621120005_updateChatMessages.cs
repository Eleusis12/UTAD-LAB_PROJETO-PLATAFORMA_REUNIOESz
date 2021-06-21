using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class updateChatMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_RegisteredUsers_Sender",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Sender",
                table: "Messages");

            migrationBuilder.AlterColumn<string>(
                name: "Sender",
                table: "Messages",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Sender",
                table: "Messages",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Sender",
                table: "Messages",
                column: "Sender");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_RegisteredUsers_Sender",
                table: "Messages",
                column: "Sender",
                principalTable: "RegisteredUsers",
                principalColumn: "RegisteredUserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
