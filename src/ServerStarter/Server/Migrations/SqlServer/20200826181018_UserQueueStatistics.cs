using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Migrations.SqlServer
{
    public partial class UserQueueStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserQueueStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    TimeInQueue = table.Column<TimeSpan>(nullable: false),
                    ServersJoined = table.Column<int>(nullable: false),
                    ServersJoinMissed = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQueueStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserQueueStatistics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserQueueStatistics_UserId",
                table: "UserQueueStatistics",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQueueStatistics");
        }
    }
}
