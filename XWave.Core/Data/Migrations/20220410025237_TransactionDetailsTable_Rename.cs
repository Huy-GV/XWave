using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XWave.Web.Data.Migrations;

public partial class TransactionDetailsTable_Rename : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_TransactionDetails_CustomerAccount_CustomerId",
            table: "TransactionDetails");

        migrationBuilder.DropForeignKey(
            name: "FK_TransactionDetails_PaymentAccount_PaymentAccountId",
            table: "TransactionDetails");

        migrationBuilder.DropPrimaryKey(
            name: "PK_TransactionDetails",
            table: "TransactionDetails");

        migrationBuilder.DropIndex(
            name: "IX_TransactionDetails_PaymentAccountId",
            table: "TransactionDetails");

        migrationBuilder.RenameTable(
            name: "TransactionDetails",
            newName: "PaymentAccountDetails");

        migrationBuilder.AddPrimaryKey(
            name: "PK_PaymentAccountDetails",
            table: "PaymentAccountDetails",
            columns: new[] { "CustomerId", "PaymentAccountId" });

        migrationBuilder.CreateIndex(
            name: "IX_PaymentAccountDetails_PaymentAccountId",
            table: "PaymentAccountDetails",
            column: "PaymentAccountId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_PaymentAccountDetails_CustomerAccount_CustomerId",
            table: "PaymentAccountDetails",
            column: "CustomerId",
            principalTable: "CustomerAccount",
            principalColumn: "CustomerId",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_PaymentAccountDetails_PaymentAccount_PaymentAccountId",
            table: "PaymentAccountDetails",
            column: "PaymentAccountId",
            principalTable: "PaymentAccount",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_PaymentAccountDetails_CustomerAccount_CustomerId",
            table: "PaymentAccountDetails");

        migrationBuilder.DropForeignKey(
            name: "FK_PaymentAccountDetails_PaymentAccount_PaymentAccountId",
            table: "PaymentAccountDetails");

        migrationBuilder.DropPrimaryKey(
            name: "PK_PaymentAccountDetails",
            table: "PaymentAccountDetails");

        migrationBuilder.DropIndex(
            name: "IX_PaymentAccountDetails_PaymentAccountId",
            table: "PaymentAccountDetails");

        migrationBuilder.RenameTable(
            name: "PaymentAccountDetails",
            newName: "TransactionDetails");

        migrationBuilder.AddPrimaryKey(
            name: "PK_TransactionDetails",
            table: "TransactionDetails",
            columns: new[] { "CustomerId", "PaymentAccountId" });

        migrationBuilder.CreateIndex(
            name: "IX_TransactionDetails_PaymentAccountId",
            table: "TransactionDetails",
            column: "PaymentAccountId");

        migrationBuilder.AddForeignKey(
            name: "FK_TransactionDetails_CustomerAccount_CustomerId",
            table: "TransactionDetails",
            column: "CustomerId",
            principalTable: "CustomerAccount",
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
}
