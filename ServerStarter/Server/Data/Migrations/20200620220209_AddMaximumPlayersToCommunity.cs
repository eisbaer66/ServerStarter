using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Data.Migrations
{
    public partial class AddMaximumPlayersToCommunity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaximumPlayers",
                table: "Communities",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaximumPlayers",
                table: "Communities");
        }
    }
}
