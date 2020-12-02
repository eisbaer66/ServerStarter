using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Migrations.SqlServer
{
    public partial class net50 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [dbo].[CommunityQueueEntry] WHERE [QueueId] IS NULL OR [UserId] IS NULL");
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityQueueEntry_CommunitiesQueues_QueueId",
                table: "CommunityQueueEntry");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsumedTime",
                table: "PersistedGrants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PersistedGrants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "PersistedGrants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "DeviceCodes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "DeviceCodes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "QueueId",
                table: "CommunityQueueEntry",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersistedGrants_SubjectId_SessionId_Type",
                table: "PersistedGrants",
                columns: new[] { "SubjectId", "SessionId", "Type" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityQueueEntry_CommunitiesQueues_QueueId",
                table: "CommunityQueueEntry",
                column: "QueueId",
                principalTable: "CommunitiesQueues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunityQueueEntry_CommunitiesQueues_QueueId",
                table: "CommunityQueueEntry");

            migrationBuilder.DropIndex(
                name: "IX_PersistedGrants_SubjectId_SessionId_Type",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "ConsumedTime",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "PersistedGrants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "DeviceCodes");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "DeviceCodes");

            migrationBuilder.AlterColumn<Guid>(
                name: "QueueId",
                table: "CommunityQueueEntry",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityQueueEntry_CommunitiesQueues_QueueId",
                table: "CommunityQueueEntry",
                column: "QueueId",
                principalTable: "CommunitiesQueues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
