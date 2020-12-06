using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ServerStarter.Server.Migrations.MySql
{
    public partial class CommunityHeaderImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "HeaderImage",
                table: "Communities",
                type: "longblob",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderImageContentType",
                table: "Communities",
                type: "longtext CHARACTER SET utf8mb4",
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
