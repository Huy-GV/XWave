using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XWave.Web.Data.Migrations
{
    public partial class PaymentAccount_SoftDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "PaymentAccount",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PaymentAccount",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "PaymentAccount");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PaymentAccount");
        }
    }
}
