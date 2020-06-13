using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Data.Migrations
{
    public partial class AddSteamIdToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SteamId",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SteamId",
                table: "AspNetUsers");
        }
    }
}
