using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class Order_Related_Models : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Country",
            table: "AspNetUsers");

        migrationBuilder.AlterColumn<decimal>(
            name: "Price",
            table: "Products",
            type: "decimal(18,4)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AddColumn<int>(
            name: "CustomerID",
            table: "AspNetUsers",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "FirstName",
            table: "AspNetUsers",
            type: "nvarchar(15)",
            maxLength: 15,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "LastName",
            table: "AspNetUsers",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "Payment",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                AccountNumber = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payment", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Customer",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Country = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                PaymentID = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Customer", x => x.ID);
                table.ForeignKey(
                    name: "FK_Customer_Payment_PaymentID",
                    column: x => x.PaymentID,
                    principalTable: "Payment",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Order",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                CustomerID = table.Column<int>(type: "int", nullable: false),
                PaymentID = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Order", x => x.ID);
                table.ForeignKey(
                    name: "FK_Order_Customer_CustomerID",
                    column: x => x.CustomerID,
                    principalTable: "Customer",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Order_Payment_PaymentID",
                    column: x => x.PaymentID,
                    principalTable: "Payment",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "OrderDetail",
            columns: table => new
            {
                OrderID = table.Column<int>(type: "int", nullable: false),
                ProductID = table.Column<int>(type: "int", nullable: false),
                PriceAtOrder = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                Quantity = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrderDetail", x => new { x.OrderID, x.ProductID });
                table.ForeignKey(
                    name: "FK_OrderDetail_Order_OrderID",
                    column: x => x.OrderID,
                    principalTable: "Order",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_OrderDetail_Products_ProductID",
                    column: x => x.ProductID,
                    principalTable: "Products",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_CustomerID",
            table: "AspNetUsers",
            column: "CustomerID");

        migrationBuilder.CreateIndex(
            name: "IX_Customer_PaymentID",
            table: "Customer",
            column: "PaymentID");

        migrationBuilder.CreateIndex(
            name: "IX_Order_CustomerID",
            table: "Order",
            column: "CustomerID");

        migrationBuilder.CreateIndex(
            name: "IX_Order_PaymentID",
            table: "Order",
            column: "PaymentID");

        migrationBuilder.CreateIndex(
            name: "IX_OrderDetail_ProductID",
            table: "OrderDetail",
            column: "ProductID");

        migrationBuilder.AddForeignKey(
            name: "FK_AspNetUsers_Customer_CustomerID",
            table: "AspNetUsers",
            column: "CustomerID",
            principalTable: "Customer",
            principalColumn: "ID",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_AspNetUsers_Customer_CustomerID",
            table: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "OrderDetail");

        migrationBuilder.DropTable(
            name: "Order");

        migrationBuilder.DropTable(
            name: "Customer");

        migrationBuilder.DropTable(
            name: "Payment");

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_CustomerID",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "CustomerID",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "FirstName",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "LastName",
            table: "AspNetUsers");

        migrationBuilder.AlterColumn<int>(
            name: "Price",
            table: "Products",
            type: "int",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,4)");

        migrationBuilder.AddColumn<string>(
            name: "Country",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}