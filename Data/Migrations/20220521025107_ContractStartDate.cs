using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XWave.Data.Migrations
{
    public partial class ContractStartDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "StaffAccount",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "StaffAccount");
        }
    }
}
