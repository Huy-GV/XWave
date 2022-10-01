using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class Nullable_ForeignKey_ProductDiscount : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Product_Discount_DiscountID",
            table: "Product");

        migrationBuilder.AddForeignKey(
            name: "FK_Product_Discount_DiscountID",
            table: "Product",
            column: "DiscountID",
            principalTable: "Discount",
            principalColumn: "ID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Product_Discount_DiscountID",
            table: "Product");

        migrationBuilder.AddForeignKey(
            name: "FK_Product_Discount_DiscountID",
            table: "Product",
            column: "DiscountID",
            principalTable: "Discount",
            principalColumn: "ID",
            onDelete: ReferentialAction.Restrict);
    }
}