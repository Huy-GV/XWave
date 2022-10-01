using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XWave.Web.Data.Migrations;

public partial class TransactionDetails_Remove_NonsenseFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LatestPurchase",
            table: "TransactionDetails");

        migrationBuilder.DropColumn(
            name: "PurchaseCount",
            table: "TransactionDetails");

        migrationBuilder.DropColumn(
            name: "TransactionType",
            table: "TransactionDetails");

        migrationBuilder.RenameColumn(
            name: "Registration",
            table: "TransactionDetails",
            newName: "FirstRegistration");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "FirstRegistration",
            table: "TransactionDetails",
            newName: "Registration");

        migrationBuilder.AddColumn<DateTime>(
            name: "LatestPurchase",
            table: "TransactionDetails",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "PurchaseCount",
            table: "TransactionDetails",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<string>(
            name: "TransactionType",
            table: "TransactionDetails",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
