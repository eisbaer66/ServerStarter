using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Data.Migrations
{
    public partial class AddOrderToCommunitiesAndCommunityServers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "CommunityServer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Communities",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "CommunityServer");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Communities");
        }
    }
}
