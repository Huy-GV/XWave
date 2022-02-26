using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class database_remodelling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discount_AspNetUsers_ManagerID",
                table: "Discount");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Customer_CustomerID",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentID",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Order_OrderID",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Product_ProductID",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Customer_CustomerID",
                table: "PaymentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentID",
                table: "PaymentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffActivityLog_AspNetUsers_StaffID",
                table: "StaffActivityLog");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "StaffID",
                table: "StaffActivityLog",
                newName: "StaffId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "StaffActivityLog",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_StaffActivityLog_StaffID",
                table: "StaffActivityLog",
                newName: "IX_StaffActivityLog_StaffId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Product",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "PaymentDetail",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "PaymentID",
                table: "PaymentDetail",
                newName: "PaymentAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDetail_PaymentID",
                table: "PaymentDetail",
                newName: "IX_PaymentDetail_PaymentAccountId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Payment",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ProductID",
                table: "OrderDetail",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "OrderID",
                table: "OrderDetail",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_ProductID",
                table: "OrderDetail",
                newName: "IX_OrderDetail_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_OrderID",
                table: "OrderDetail",
                newName: "IX_OrderDetail_OrderId");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "Order",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Order",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PaymentID",
                table: "Order",
                newName: "PaymentAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_CustomerID",
                table: "Order",
                newName: "IX_Order_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_PaymentID",
                table: "Order",
                newName: "IX_Order_PaymentAccountId");

            migrationBuilder.RenameColumn(
                name: "ManagerID",
                table: "Discount",
                newName: "ManagerId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Discount",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Discount_ManagerID",
                table: "Discount",
                newName: "IX_Discount_ManagerId");

            migrationBuilder.RenameColumn(
                name: "CustomerID",
                table: "Customer",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Category",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "PaymentDetail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNo",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "BillingAddress",
                table: "Customer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ContractStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Salary = table.Column<long>(type: "bigint", nullable: false),
                    HoursPerWeek = table.Column<long>(type: "bigint", nullable: false),
                    ManagerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.StaffId);
                    table.ForeignKey(
                        name: "FK_Staff_AspNetUsers_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Staff_AspNetUsers_StaffId",
                        column: x => x.StaffId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ManagerId",
                table: "Staff",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_AspNetUsers_CustomerId",
                table: "Customer",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Discount_AspNetUsers_ManagerId",
                table: "Discount",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentAccountId",
                table: "Order",
                column: "PaymentAccountId",
                principalTable: "Payment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Order_OrderId",
                table: "OrderDetail",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Product_ProductId",
                table: "OrderDetail",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Customer_CustomerId",
                table: "PaymentDetail",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentAccountId",
                table: "PaymentDetail",
                column: "PaymentAccountId",
                principalTable: "Payment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffActivityLog_AspNetUsers_StaffId",
                table: "StaffActivityLog",
                column: "StaffId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_AspNetUsers_CustomerId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Discount_AspNetUsers_ManagerId",
                table: "Discount");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentAccountId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Order_OrderId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Product_ProductId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Customer_CustomerId",
                table: "PaymentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentAccountId",
                table: "PaymentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffActivityLog_AspNetUsers_StaffId",
                table: "StaffActivityLog");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "BillingAddress",
                table: "Customer");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "StaffActivityLog",
                newName: "StaffID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StaffActivityLog",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_StaffActivityLog_StaffId",
                table: "StaffActivityLog",
                newName: "IX_StaffActivityLog_StaffID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Product",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "PaymentDetail",
                newName: "CustomerID");

            migrationBuilder.RenameColumn(
                name: "PaymentAccountId",
                table: "PaymentDetail",
                newName: "PaymentID");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDetail_PaymentAccountId",
                table: "PaymentDetail",
                newName: "IX_PaymentDetail_PaymentID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Payment",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "OrderDetail",
                newName: "ProductID");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "OrderDetail",
                newName: "OrderID");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_ProductId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_ProductID");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                newName: "IX_OrderDetail_OrderID");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Order",
                newName: "CustomerID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Order",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "PaymentAccountId",
                table: "Order",
                newName: "PaymentID");

            migrationBuilder.RenameIndex(
                name: "IX_Order_CustomerId",
                table: "Order",
                newName: "IX_Order_CustomerID");

            migrationBuilder.RenameIndex(
                name: "IX_Order_PaymentAccountId",
                table: "Order",
                newName: "IX_Order_PaymentID");

            migrationBuilder.RenameColumn(
                name: "ManagerId",
                table: "Discount",
                newName: "ManagerID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Discount",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_Discount_ManagerId",
                table: "Discount",
                newName: "IX_Discount_ManagerID");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Customer",
                newName: "CustomerID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Category",
                newName: "ID");

            migrationBuilder.AlterColumn<long>(
                name: "AccountNo",
                table: "Payment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Customer",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Discount_AspNetUsers_ManagerID",
                table: "Discount",
                column: "ManagerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Customer_CustomerID",
                table: "Order",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentID",
                table: "Order",
                column: "PaymentID",
                principalTable: "Payment",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Order_OrderID",
                table: "OrderDetail",
                column: "OrderID",
                principalTable: "Order",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Product_ProductID",
                table: "OrderDetail",
                column: "ProductID",
                principalTable: "Product",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Customer_CustomerID",
                table: "PaymentDetail",
                column: "CustomerID",
                principalTable: "Customer",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentID",
                table: "PaymentDetail",
                column: "PaymentID",
                principalTable: "Payment",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffActivityLog_AspNetUsers_StaffID",
                table: "StaffActivityLog",
                column: "StaffID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
