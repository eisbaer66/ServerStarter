using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Migrations.Sqlite
{
    public partial class CommunityHeaderImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "HeaderImage",
                table: "Communities",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderImageContentType",
                table: "Communities",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeaderImage",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "HeaderImageContentType",
                table: "Communities");
        }
    }
}
