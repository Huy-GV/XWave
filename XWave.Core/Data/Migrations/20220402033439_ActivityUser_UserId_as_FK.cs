using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class ActivityUser_UserId_as_FK : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Activity_AspNetUsers_UserName",
            table: "Activity");

        migrationBuilder.RenameColumn(
            name: "UserName",
            table: "Activity",
            newName: "UserId");

        migrationBuilder.RenameIndex(
            name: "IX_Activity_UserName",
            table: "Activity",
            newName: "IX_Activity_UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_Activity_AspNetUsers_UserId",
            table: "Activity",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Activity_AspNetUsers_UserId",
            table: "Activity");

        migrationBuilder.RenameColumn(
            name: "UserId",
            table: "Activity",
            newName: "UserName");

        migrationBuilder.RenameIndex(
            name: "IX_Activity_UserId",
            table: "Activity",
            newName: "IX_Activity_UserName");

        migrationBuilder.AddForeignKey(
            name: "FK_Activity_AspNetUsers_UserName",
            table: "Activity",
            column: "UserName",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}