using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Web.Data.Migrations;

public partial class Customer_and_Staff_renaming : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Customer_AspNetUsers_CustomerId",
            table: "Customer");

        migrationBuilder.DropTable(
            name: "Staff");

        migrationBuilder.CreateTable(
            name: "StaffAccount",
            columns: table => new
            {
                StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ContractStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ContractEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                Salary = table.Column<long>(type: "bigint", nullable: false),
                HoursPerWeek = table.Column<long>(type: "bigint", nullable: false),
                CreatorManagerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_StaffAccount", x => x.StaffId);
                table.ForeignKey(
                    name: "FK_StaffAccount_AspNetUsers_CreatorManagerId",
                    column: x => x.CreatorManagerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_StaffAccount_AspNetUsers_StaffId",
                    column: x => x.StaffId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_StaffAccount_CreatorManagerId",
            table: "StaffAccount",
            column: "CreatorManagerId");

        migrationBuilder.AddForeignKey(
            name: "FK_Customer_AspNetUsers_CustomerId",
            table: "Customer",
            column: "CustomerId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Customer_AspNetUsers_CustomerId",
            table: "Customer");

        migrationBuilder.DropTable(
            name: "StaffAccount");

        migrationBuilder.CreateTable(
            name: "Staff",
            columns: table => new
            {
                StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                ContractEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                ContractStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                HoursPerWeek = table.Column<long>(type: "bigint", nullable: false),
                ManagerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Salary = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Staff", x => x.StaffId);
                table.ForeignKey(
                    name: "FK_Staff_AspNetUsers_ManagerId",
                    column: x => x.ManagerId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Staff_AspNetUsers_StaffId",
                    column: x => x.StaffId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Staff_ManagerId",
            table: "Staff",
            column: "ManagerId");

        migrationBuilder.AddForeignKey(
            name: "FK_Customer_AspNetUsers_CustomerId",
            table: "Customer",
            column: "CustomerId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}