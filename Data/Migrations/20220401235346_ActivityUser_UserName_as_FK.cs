using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class ActivityUser_UserName_as_FK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_AspNetUsers_AppUserId",
                table: "Activity");

            migrationBuilder.RenameColumn(
                name: "AppUserId",
                table: "Activity",
                newName: "UserName");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_AppUserId",
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_AspNetUsers_UserName",
                table: "Activity");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Activity",
                newName: "AppUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Activity_UserName",
                table: "Activity",
                newName: "IX_Activity_AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_AspNetUsers_AppUserId",
                table: "Activity",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
