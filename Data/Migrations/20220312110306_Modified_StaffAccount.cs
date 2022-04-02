using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace XWave.Data.Migrations
{
    public partial class Modified_StaffAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_AspNetUsers_CustomerId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffAccount_AspNetUsers_CreatorManagerId",
                table: "StaffAccount");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_Customer_CustomerId",
                table: "TransactionDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customer",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "HoursPerWeek",
                table: "StaffAccount");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "Customer",
                newName: "CustomerAccount");

            migrationBuilder.RenameColumn(
                name: "Salary",
                table: "StaffAccount",
                newName: "HourlyWage");

            migrationBuilder.RenameColumn(
                name: "CreatorManagerId",
                table: "StaffAccount",
                newName: "ImmediateManagerId");

            migrationBuilder.RenameIndex(
                name: "IX_StaffAccount_CreatorManagerId",
                table: "StaffAccount",
                newName: "IX_StaffAccount_ImmediateManagerId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegistrationDate",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerAccount",
                table: "CustomerAccount",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAccount_AspNetUsers_CustomerId",
                table: "CustomerAccount",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_CustomerAccount_CustomerId",
                table: "Order",
                column: "CustomerId",
                principalTable: "CustomerAccount",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffAccount_AspNetUsers_ImmediateManagerId",
                table: "StaffAccount",
                column: "ImmediateManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_CustomerAccount_CustomerId",
                table: "TransactionDetails",
                column: "CustomerId",
                principalTable: "CustomerAccount",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAccount_AspNetUsers_CustomerId",
                table: "CustomerAccount");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_CustomerAccount_CustomerId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_StaffAccount_AspNetUsers_ImmediateManagerId",
                table: "StaffAccount");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionDetails_CustomerAccount_CustomerId",
                table: "TransactionDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerAccount",
                table: "CustomerAccount");

            migrationBuilder.RenameTable(
                name: "CustomerAccount",
                newName: "Customer");

            migrationBuilder.RenameColumn(
                name: "ImmediateManagerId",
                table: "StaffAccount",
                newName: "CreatorManagerId");

            migrationBuilder.RenameColumn(
                name: "HourlyWage",
                table: "StaffAccount",
                newName: "Salary");

            migrationBuilder.RenameIndex(
                name: "IX_StaffAccount_ImmediateManagerId",
                table: "StaffAccount",
                newName: "IX_StaffAccount_CreatorManagerId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "StaffAccount",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "HoursPerWeek",
                table: "StaffAccount",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RegistrationDate",
                table: "AspNetUsers",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customer",
                table: "Customer",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_AspNetUsers_CustomerId",
                table: "Customer",
                column: "CustomerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffAccount_AspNetUsers_CreatorManagerId",
                table: "StaffAccount",
                column: "CreatorManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionDetails_Customer_CustomerId",
                table: "TransactionDetails",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}