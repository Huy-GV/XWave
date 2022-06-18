using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace XWave.Web.Data.Migrations
{
    public partial class Inventory_Product_Merge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Inventory_InventoryID",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropIndex(
                name: "IX_Products_InventoryID",
                table: "Products");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRestock",
                table: "Products",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRestock",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastRestock = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_InventoryID",
                table: "Products",
                column: "InventoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Inventory_InventoryID",
                table: "Products",
                column: "InventoryID",
                principalTable: "Inventory",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}