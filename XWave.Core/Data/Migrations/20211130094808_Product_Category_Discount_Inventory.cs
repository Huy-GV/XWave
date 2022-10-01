using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class Product_Category_Discount_Inventory : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Category",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                Description = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Category", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Discount",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Percentage = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Discount", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Inventory",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Quantity = table.Column<int>(type: "int", nullable: false),
                LastRestock = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Inventory", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                Price = table.Column<int>(type: "int", nullable: false),
                ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                InventoryID = table.Column<int>(type: "int", nullable: false),
                CategoryID = table.Column<int>(type: "int", nullable: true),
                DiscountID = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.ID);
                table.ForeignKey(
                    name: "FK_Products_Category_CategoryID",
                    column: x => x.CategoryID,
                    principalTable: "Category",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Products_Discount_DiscountID",
                    column: x => x.DiscountID,
                    principalTable: "Discount",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Products_Inventory_InventoryID",
                    column: x => x.InventoryID,
                    principalTable: "Inventory",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Products_CategoryID",
            table: "Products",
            column: "CategoryID");

        migrationBuilder.CreateIndex(
            name: "IX_Products_DiscountID",
            table: "Products",
            column: "DiscountID");

        migrationBuilder.CreateIndex(
            name: "IX_Products_InventoryID",
            table: "Products",
            column: "InventoryID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Products");

        migrationBuilder.DropTable(
            name: "Category");

        migrationBuilder.DropTable(
            name: "Discount");

        migrationBuilder.DropTable(
            name: "Inventory");
    }
}