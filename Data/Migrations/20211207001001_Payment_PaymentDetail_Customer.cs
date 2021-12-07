using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class Payment_PaymentDetail_Customer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Payment_PaymentID",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentID",
                table: "Order");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Order_PaymentID",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Customer_PaymentID",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<long>(
                name: "AccountNo",
                table: "Payment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "AccountNo",
                table: "Order",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PaymentAccountNo",
                table: "Order",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "Order",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Customer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhoneNumber",
                table: "Customer",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                columns: new[] { "AccountNo", "Provider" });

            migrationBuilder.CreateTable(
                name: "PaymentDetail",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    AccountNo = table.Column<long>(type: "bigint", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Registration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseCount = table.Column<int>(type: "int", nullable: false),
                    LatestPurchase = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentAccountNo = table.Column<long>(type: "bigint", nullable: true),
                    PaymentProvider = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetail", x => new { x.CustomerID, x.AccountNo, x.Provider });
                    table.ForeignKey(
                        name: "FK_PaymentDetail_Customer_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customer",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDetail_Payment_PaymentAccountNo_PaymentProvider",
                        columns: x => new { x.PaymentAccountNo, x.PaymentProvider },
                        principalTable: "Payment",
                        principalColumns: new[] { "AccountNo", "Provider" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_PaymentAccountNo_PaymentProvider",
                table: "Order",
                columns: new[] { "PaymentAccountNo", "PaymentProvider" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_PaymentAccountNo_PaymentProvider",
                table: "PaymentDetail",
                columns: new[] { "PaymentAccountNo", "PaymentProvider" });

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentAccountNo_PaymentProvider",
                table: "Order",
                columns: new[] { "PaymentAccountNo", "PaymentProvider" },
                principalTable: "Payment",
                principalColumns: new[] { "AccountNo", "Provider" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentAccountNo_PaymentProvider",
                table: "Order");

            migrationBuilder.DropTable(
                name: "PaymentDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Order_PaymentAccountNo_PaymentProvider",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AccountNo",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "AccountNo",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PaymentAccountNo",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customer");

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "ID",
                table: "Payment",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "AccountNumber",
                table: "Payment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentID",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentID",
                table: "Customer",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PaymentID",
                table: "Order",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_PaymentID",
                table: "Customer",
                column: "PaymentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Payment_PaymentID",
                table: "Customer",
                column: "PaymentID",
                principalTable: "Payment",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentID",
                table: "Order",
                column: "PaymentID",
                principalTable: "Payment",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
