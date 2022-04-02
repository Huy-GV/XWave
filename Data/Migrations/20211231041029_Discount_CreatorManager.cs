using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class Discount_CreatorManager : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ManagerID",
                table: "Discount",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Discount_ManagerID",
                table: "Discount",
                column: "ManagerID");

            migrationBuilder.AddForeignKey(
                name: "FK_Discount_AspNetUsers_ManagerID",
                table: "Discount",
                column: "ManagerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discount_AspNetUsers_ManagerID",
                table: "Discount");

            migrationBuilder.DropIndex(
                name: "IX_Discount_ManagerID",
                table: "Discount");

            migrationBuilder.DropColumn(
                name: "ManagerID",
                table: "Discount");
        }
    }
}