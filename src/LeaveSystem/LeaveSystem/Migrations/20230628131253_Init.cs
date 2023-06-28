using System;
using System.Text.Json;
using LeaveSystem.Db.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveSystem.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseLeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Properties = table.Column<LeaveType.LeaveTypeProperties>(type: "jsonb", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveTypes_LeaveTypes_BaseLeaveTypeId",
                        column: x => x.BaseLeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLeaveLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Limit = table.Column<TimeSpan>(type: "interval", nullable: true),
                    OverdueLimit = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AssignedToUserId = table.Column<string>(type: "text", nullable: true),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Property = table.Column<UserLeaveLimit.UserLeaveLimitProperties>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeaveLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLeaveLimits_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_BaseLeaveTypeId",
                table: "LeaveTypes",
                column: "BaseLeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_Name",
                table: "LeaveTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaveLimits_LeaveTypeId_AssignedToUserId_ValidSince",
                table: "UserLeaveLimits",
                columns: new[] { "LeaveTypeId", "AssignedToUserId", "ValidSince" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaveLimits_LeaveTypeId_AssignedToUserId_ValidUntil",
                table: "UserLeaveLimits",
                columns: new[] { "LeaveTypeId", "AssignedToUserId", "ValidUntil" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UserLeaveLimits");

            migrationBuilder.DropTable(
                name: "LeaveTypes");
        }
    }
}
