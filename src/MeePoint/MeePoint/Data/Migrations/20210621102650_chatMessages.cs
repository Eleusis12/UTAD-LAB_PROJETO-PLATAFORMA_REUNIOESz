using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class chatMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Meetings",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sender = table.Column<int>(nullable: false),
                    MeetingID = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageID);
                    table.ForeignKey(
                        name: "FK_Messages_Meetings_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "Meetings",
                        principalColumn: "MeetingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_RegisteredUsers_Sender",
                        column: x => x.Sender,
                        principalTable: "RegisteredUsers",
                        principalColumn: "RegisteredUserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MeetingID",
                table: "Messages",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Sender",
                table: "Messages",
                column: "Sender");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Meetings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
