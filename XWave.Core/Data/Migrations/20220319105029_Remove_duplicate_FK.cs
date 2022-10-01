using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class Remove_duplicate_FK : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Product_Discount_DiscountId1",
            table: "Product");

        migrationBuilder.DropIndex(
            name: "IX_Product_DiscountId1",
            table: "Product");

        migrationBuilder.DropColumn(
            name: "DiscountId1",
            table: "Product");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "DiscountId1",
            table: "Product",
            type: "int",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Product_DiscountId1",
            table: "Product",
            column: "DiscountId1");

        migrationBuilder.AddForeignKey(
            name: "FK_Product_Discount_DiscountId1",
            table: "Product",
            column: "DiscountId1",
            principalTable: "Discount",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}