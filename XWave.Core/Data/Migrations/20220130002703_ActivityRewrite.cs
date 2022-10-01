using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class ActivityRewrite : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_StaffActivityLog_Activity_ActivityID",
            table: "StaffActivityLog");

        migrationBuilder.DropIndex(
            name: "IX_StaffActivityLog_ActivityID",
            table: "StaffActivityLog");

        migrationBuilder.DropColumn(
            name: "ActivityID",
            table: "StaffActivityLog");

        migrationBuilder.RenameColumn(
            name: "Message",
            table: "StaffActivityLog",
            newName: "EntityType");

        migrationBuilder.AddColumn<string>(
            name: "ActionType",
            table: "StaffActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ActionType",
            table: "StaffActivityLog");

        migrationBuilder.RenameColumn(
            name: "EntityType",
            table: "StaffActivityLog",
            newName: "Message");

        migrationBuilder.AddColumn<int>(
            name: "ActivityID",
            table: "StaffActivityLog",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_StaffActivityLog_ActivityID",
            table: "StaffActivityLog",
            column: "ActivityID");

        migrationBuilder.AddForeignKey(
            name: "FK_StaffActivityLog_Activity_ActivityID",
            table: "StaffActivityLog",
            column: "ActivityID",
            principalTable: "Activity",
            principalColumn: "ID",
            onDelete: ReferentialAction.Cascade);
    }
}