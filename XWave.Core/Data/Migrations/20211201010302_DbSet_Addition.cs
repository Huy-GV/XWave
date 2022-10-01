using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class DbSet_Addition : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_Category_CategoryID",
            table: "Products");

        migrationBuilder.DropForeignKey(
            name: "FK_Products_Discount_DiscountID",
            table: "Products");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Discount",
            table: "Discount");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Category",
            table: "Category");

        migrationBuilder.RenameTable(
            name: "Discount",
            newName: "Discounts");

        migrationBuilder.RenameTable(
            name: "Category",
            newName: "Categories");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Discounts",
            table: "Discounts",
            column: "ID");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Categories",
            table: "Categories",
            column: "ID");

        migrationBuilder.AddForeignKey(
            name: "FK_Products_Categories_CategoryID",
            table: "Products",
            column: "CategoryID",
            principalTable: "Categories",
            principalColumn: "ID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Products_Discounts_DiscountID",
            table: "Products",
            column: "DiscountID",
            principalTable: "Discounts",
            principalColumn: "ID",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Products_Categories_CategoryID",
            table: "Products");

        migrationBuilder.DropForeignKey(
            name: "FK_Products_Discounts_DiscountID",
            table: "Products");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Discounts",
            table: "Discounts");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Categories",
            table: "Categories");

        migrationBuilder.RenameTable(
            name: "Discounts",
            newName: "Discount");

        migrationBuilder.RenameTable(
            name: "Categories",
            newName: "Category");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Discount",
            table: "Discount",
            column: "ID");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Category",
            table: "Category",
            column: "ID");

        migrationBuilder.AddForeignKey(
            name: "FK_Products_Category_CategoryID",
            table: "Products",
            column: "CategoryID",
            principalTable: "Category",
            principalColumn: "ID",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Products_Discount_DiscountID",
            table: "Products",
            column: "DiscountID",
            principalTable: "Discount",
            principalColumn: "ID",
            onDelete: ReferentialAction.Restrict);
    }
}