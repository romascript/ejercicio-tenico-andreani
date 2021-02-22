using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ApiGeo.Migrations
{
    public partial class MigracionAddAtCreated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "At_Created",
                table: "SolicitudItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "At_Created",
                table: "SolicitudItems");
        }
    }
}
