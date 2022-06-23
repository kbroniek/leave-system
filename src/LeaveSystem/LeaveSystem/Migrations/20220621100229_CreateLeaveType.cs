using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveSystem.Migrations
{
    public partial class CreateLeaveType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseLeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Properties = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.LeaveTypeId);
                    table.ForeignKey(
                        name: "FK_LeaveTypes_LeaveTypes_BaseLeaveTypeId",
                        column: x => x.BaseLeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_BaseLeaveTypeId",
                table: "LeaveTypes",
                column: "BaseLeaveTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveTypes");
        }
    }
}
