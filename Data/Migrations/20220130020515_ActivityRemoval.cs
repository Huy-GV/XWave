using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace XWave.Data.Migrations
{
    public partial class ActivityRemoval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatingManagerID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Activity_AspNetUsers_CreatingManagerID",
                        column: x => x.CreatingManagerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activity_CreatingManagerID",
                table: "Activity",
                column: "CreatingManagerID");
        }
    }
}
