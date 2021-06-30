using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MeePoint.Data.Migrations
{
    public partial class SpeakRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpeakRequests",
                columns: table => new
                {
                    SpeakRequestID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WhoRequestedRegisteredUserID = table.Column<int>(nullable: true),
                    MeetingID = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeakRequests", x => x.SpeakRequestID);
                    table.ForeignKey(
                        name: "FK_SpeakRequests_Meetings_MeetingID",
                        column: x => x.MeetingID,
                        principalTable: "Meetings",
                        principalColumn: "MeetingID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeakRequests_RegisteredUsers_WhoRequestedRegisteredUserID",
                        column: x => x.WhoRequestedRegisteredUserID,
                        principalTable: "RegisteredUsers",
                        principalColumn: "RegisteredUserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpeakRequests_MeetingID",
                table: "SpeakRequests",
                column: "MeetingID");

            migrationBuilder.CreateIndex(
                name: "IX_SpeakRequests_WhoRequestedRegisteredUserID",
                table: "SpeakRequests",
                column: "WhoRequestedRegisteredUserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeakRequests");
        }
    }
}
