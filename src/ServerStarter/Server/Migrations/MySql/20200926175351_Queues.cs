using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Migrations.MySql
{
    public partial class Queues : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunitiesQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CommunityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunitiesQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunitiesQueues_Communities_CommunityId",
                        column: x => x.CommunityId,
                        principalTable: "Communities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommunityQueueEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    QueueId = table.Column<Guid>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityQueueEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityQueueEntry_CommunitiesQueues_QueueId",
                        column: x => x.QueueId,
                        principalTable: "CommunitiesQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityQueueEntry_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunitiesQueues_CommunityId",
                table: "CommunitiesQueues",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityQueueEntry_QueueId",
                table: "CommunityQueueEntry",
                column: "QueueId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityQueueEntry_UserId",
                table: "CommunityQueueEntry",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityQueueEntry");

            migrationBuilder.DropTable(
                name: "CommunitiesQueues");
        }
    }
}
