using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Migrations.MySql
{
    public partial class UserQueueStatisticsRequiresIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQueueStatistics_AspNetUsers_UserId",
                table: "UserQueueStatistics");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserQueueStatistics",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255) CHARACTER SET utf8mb4",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserQueueStatistics",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string));

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
