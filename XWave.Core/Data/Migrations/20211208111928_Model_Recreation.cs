using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class Model_Recreation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Payment",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AccountNo = table.Column<long>(type: "bigint", nullable: false),
                Provider = table.Column<string>(type: "nvarchar(450)", nullable: true),
                ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Payment", x => x.ID);
            });

        migrationBuilder.CreateTable(
            name: "Order",
            columns: table => new
            {
                ID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                CustomerID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                PaymentID = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Order", x => x.ID);
                table.ForeignKey(
                    name: "FK_Order_Customer_CustomerID",
                    column: x => x.CustomerID,
                    principalTable: "Customer",
                    principalColumn: "CustomerID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Order_Payment_PaymentID",
                    column: x => x.PaymentID,
                    principalTable: "Payment",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PaymentDetail",
            columns: table => new
            {
                CustomerID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                PaymentID = table.Column<int>(type: "int", nullable: false),
                Registration = table.Column<DateTime>(type: "datetime2", nullable: false),
                PurchaseCount = table.Column<long>(type: "bigint", nullable: false),
                LatestPurchase = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PaymentDetail", x => new { x.CustomerID, x.PaymentID });
                table.ForeignKey(
                    name: "FK_PaymentDetail_Customer_CustomerID",
                    column: x => x.CustomerID,
                    principalTable: "Customer",
                    principalColumn: "CustomerID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PaymentDetail_Payment_PaymentID",
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
                    name: "FK_OrderDetail_Product_ProductID",
                    column: x => x.ProductID,
                    principalTable: "Product",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
            });

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

        migrationBuilder.CreateIndex(
            name: "IX_Payment_AccountNo_Provider",
            table: "Payment",
            columns: new[] { "AccountNo", "Provider" },
            unique: true,
            filter: "[Provider] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_PaymentDetail_PaymentID",
            table: "PaymentDetail",
            column: "PaymentID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OrderDetail");

        migrationBuilder.DropTable(
            name: "PaymentDetail");

        migrationBuilder.DropTable(
            name: "Order");

        migrationBuilder.DropTable(
            name: "Payment");
    }
}