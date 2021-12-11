using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class OrderDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderID",
                table: "OrderDetail",
                column: "OrderID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderDetail_OrderID",
                table: "OrderDetail");
        }
    }
}
