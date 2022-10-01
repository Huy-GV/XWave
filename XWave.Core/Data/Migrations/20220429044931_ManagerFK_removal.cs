using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XWave.Web.Data.Migrations;

public partial class ManagerFK_removal : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Discount_AspNetUsers_ManagerId",
            table: "Discount");

        migrationBuilder.DropIndex(
            name: "IX_Discount_ManagerId",
            table: "Discount");

        migrationBuilder.DropColumn(
            name: "ManagerId",
            table: "Discount");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ManagerId",
            table: "Discount",
            type: "nvarchar(450)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_Discount_ManagerId",
            table: "Discount",
            column: "ManagerId");

        migrationBuilder.AddForeignKey(
            name: "FK_Discount_AspNetUsers_ManagerId",
            table: "Discount",
            column: "ManagerId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
