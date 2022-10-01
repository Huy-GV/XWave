using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class Remove_OrderID_Index : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_OrderDetail_OrderID",
            table: "OrderDetail");

        migrationBuilder.CreateIndex(
            name: "IX_OrderDetail_OrderID",
            table: "OrderDetail",
            column: "OrderID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_OrderDetail_OrderID",
            table: "OrderDetail");

        migrationBuilder.CreateIndex(
            name: "IX_OrderDetail_OrderID",
            table: "OrderDetail",
            column: "OrderID",
            unique: true);
    }
}