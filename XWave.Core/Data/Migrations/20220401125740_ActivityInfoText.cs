using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class ActivityInfoText : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Activity_AspNetUsers_StaffId",
            table: "Activity");

        migrationBuilder.DropIndex(
            name: "IX_OrderDetails_OrderId",
            table: "OrderDetails");

        migrationBuilder.RenameColumn(
            name: "StaffId",
            table: "Activity",
            newName: "AppUserId");

        migrationBuilder.RenameIndex(
            name: "IX_Activity_StaffId",
            table: "Activity",
            newName: "IX_Activity_AppUserId");

        migrationBuilder.AddColumn<string>(
            name: "Info",
            table: "Activity",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddForeignKey(
            name: "FK_Activity_AspNetUsers_AppUserId",
            table: "Activity",
            column: "AppUserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Activity_AspNetUsers_AppUserId",
            table: "Activity");

        migrationBuilder.DropColumn(
            name: "Info",
            table: "Activity");

        migrationBuilder.RenameColumn(
            name: "AppUserId",
            table: "Activity",
            newName: "StaffId");

        migrationBuilder.RenameIndex(
            name: "IX_Activity_AppUserId",
            table: "Activity",
            newName: "IX_Activity_StaffId");

        migrationBuilder.CreateIndex(
            name: "IX_OrderDetails_OrderId",
            table: "OrderDetails",
            column: "OrderId");

        migrationBuilder.AddForeignKey(
            name: "FK_Activity_AspNetUsers_StaffId",
            table: "Activity",
            column: "StaffId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}