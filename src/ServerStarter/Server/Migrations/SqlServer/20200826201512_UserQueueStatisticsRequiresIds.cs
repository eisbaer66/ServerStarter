using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Migrations.SqlServer
{
    public partial class UserQueueStatisticsRequiresIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQueueStatistics_AspNetUsers_UserId",
                table: "UserQueueStatistics");

            migrationBuilder.DropIndex(
                name: "IX_UserQueueStatistics_UserId",
                table: "UserQueueStatistics");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserQueueStatistics",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserQueueStatistics_UserId",
                table: "UserQueueStatistics",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserQueueStatistics_AspNetUsers_UserId",
                table: "UserQueueStatistics",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQueueStatistics_AspNetUsers_UserId",
                table: "UserQueueStatistics");

            migrationBuilder.DropIndex(
                name: "IX_UserQueueStatistics_UserId",
                table: "UserQueueStatistics");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserQueueStatistics",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_UserQueueStatistics_UserId",
                table: "UserQueueStatistics",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQueueStatistics_AspNetUsers_UserId",
                table: "UserQueueStatistics",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
