using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class Payment_Unique_Pair : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentAccountNo_PaymentProvider",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentAccountNo_PaymentProvider",
                table: "PaymentDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDetail",
                table: "PaymentDetail");

            migrationBuilder.DropIndex(
                name: "IX_PaymentDetail_PaymentAccountNo_PaymentProvider",
                table: "PaymentDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Order_PaymentAccountNo_PaymentProvider",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AccountNo",
                table: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "PaymentAccountNo",
                table: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "PaymentProvider",
                table: "PaymentDetail");

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

            migrationBuilder.AddColumn<int>(
                name: "PaymentID",
                table: "PaymentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: true,
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
                name: "PaymentID",
                table: "Order",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDetail",
                table: "PaymentDetail",
                columns: new[] { "CustomerID", "PaymentID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_PaymentID",
                table: "PaymentDetail",
                column: "PaymentID");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_AccountNo_Provider",
                table: "Payment",
                columns: new[] { "AccountNo", "Provider" },
                unique: true,
                filter: "[Provider] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Order_PaymentID",
                table: "Order",
                column: "PaymentID");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentID",
                table: "Order",
                column: "PaymentID",
                principalTable: "Payment",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentID",
                table: "PaymentDetail",
                column: "PaymentID",
                principalTable: "Payment",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_Payment_PaymentID",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentID",
                table: "PaymentDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDetail",
                table: "PaymentDetail");

            migrationBuilder.DropIndex(
                name: "IX_PaymentDetail_PaymentID",
                table: "PaymentDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payment",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Payment_AccountNo_Provider",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_Order_PaymentID",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "PaymentDetail");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PaymentID",
                table: "Order");

            migrationBuilder.AddColumn<long>(
                name: "AccountNo",
                table: "PaymentDetail",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "PaymentDetail",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "PaymentAccountNo",
                table: "PaymentDetail",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "PaymentDetail",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Provider",
                table: "Payment",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDetail",
                table: "PaymentDetail",
                columns: new[] { "CustomerID", "AccountNo", "Provider" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payment",
                table: "Payment",
                columns: new[] { "AccountNo", "Provider" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetail_PaymentAccountNo_PaymentProvider",
                table: "PaymentDetail",
                columns: new[] { "PaymentAccountNo", "PaymentProvider" });

            migrationBuilder.CreateIndex(
                name: "IX_Order_PaymentAccountNo_PaymentProvider",
                table: "Order",
                columns: new[] { "PaymentAccountNo", "PaymentProvider" });

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Payment_PaymentAccountNo_PaymentProvider",
                table: "Order",
                columns: new[] { "PaymentAccountNo", "PaymentProvider" },
                principalTable: "Payment",
                principalColumns: new[] { "AccountNo", "Provider" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetail_Payment_PaymentAccountNo_PaymentProvider",
                table: "PaymentDetail",
                columns: new[] { "PaymentAccountNo", "PaymentProvider" },
                principalTable: "Payment",
                principalColumns: new[] { "AccountNo", "Provider" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}