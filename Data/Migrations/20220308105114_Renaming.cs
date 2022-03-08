using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class Renaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentAccountId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Customer_CustomerId",
                table: "PaymentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentAccountId",
                table: "PaymentDetail");

            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "StaffActivityLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDetail",
                table: "PaymentDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.RenameTable(
                name: "PaymentDetail",
                newName: "TransactionDetails");

            migrationBuilder.RenameTable(
                name: "Payment",
                newName: "PaymentAccount");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDetail_PaymentAccountId",
                table: "TransactionDetails",
                newName: "IX_TransactionDetails_PaymentAccountId");

            migrationBuilder.RenameColumn(
                name: "AccountNo",
                table: "PaymentAccount",
                newName: "AccountNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Payment_AccountNo_Provider",
                table: "PaymentAccount",
                newName: "IX_PaymentAccount_AccountNumber_Provider");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TransactionDetails",
                table: "TransactionDetails",
                columns: new[] { "CustomerId", "PaymentAccountId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentAccount",
                table: "PaymentAccount",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Time = table.Column<DateTime>(type: "datetime", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activity_AspNetUsers_StaffId",
                        column: x => x.StaffId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PriceAtOrder = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => new { x.OrderId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_OrderDetails_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activity_StaffId",
                table: "Activity",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ProductId",
                table: "OrderDetails",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_PaymentAccount_PaymentAccountId",
                table: "Order",
                column: "PaymentAccountId",
                principalTable: "PaymentAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_Customer_CustomerId",
                table: "TransactionDetails",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_PaymentAccount_PaymentAccountId",
                table: "TransactionDetails",
                column: "PaymentAccountId",
                principalTable: "PaymentAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_PaymentAccount_PaymentAccountId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_Customer_CustomerId",
                table: "TransactionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_PaymentAccount_PaymentAccountId",
                table: "TransactionDetails");

            migrationBuilder.DropTable(
                name: "Activity");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TransactionDetails",
                table: "TransactionDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentAccount",
                table: "PaymentAccount");

            migrationBuilder.RenameTable(
                name: "TransactionDetails",
                newName: "PaymentDetail");

            migrationBuilder.RenameTable(
                name: "PaymentAccount",
                newName: "Payment");

            migrationBuilder.RenameIndex(
                name: "IX_TransactionDetails_PaymentAccountId",
                table: "PaymentDetail",
                newName: "IX_PaymentDetail_PaymentAccountId");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "Payment",
                newName: "AccountNo");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentAccount_AccountNumber_Provider",
                table: "Payment",
                newName: "IX_Payment_AccountNo_Provider");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDetail",
                table: "PaymentDetail",
                columns: new[] { "CustomerId", "PaymentAccountId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PriceAtOrder = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => new { x.OrderId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffActivityLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffActivityLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffActivityLog_AspNetUsers_StaffId",
                        column: x => x.StaffId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ProductId",
                table: "OrderDetail",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffActivityLog_StaffId",
                table: "StaffActivityLog",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentAccountId",
                table: "Order",
                column: "PaymentAccountId",
                principalTable: "Payment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Customer_CustomerId",
                table: "PaymentDetail",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentAccountId",
                table: "PaymentDetail",
                column: "PaymentAccountId",
                principalTable: "Payment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
