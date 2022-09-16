﻿using System;
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
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseLeaveTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Properties = table.Column<LeaveType.LeaveTypeProperties>(type: "jsonb", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleType = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "UserLeaveLimits",
                columns: table => new
                {
                    UserLeaveLimitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Limit = table.Column<TimeSpan>(type: "interval", nullable: true),
                    OverdueLimit = table.Column<TimeSpan>(type: "interval", nullable: true),
                    AssignedToUserEmail = table.Column<string>(type: "text", nullable: true),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidSince = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Property = table.Column<UserLeaveLimit.UserLeaveLimitProperties>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLeaveLimits", x => x.UserLeaveLimitId);
                    table.ForeignKey(
                        name: "FK_UserLeaveLimits_LeaveTypes_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveTypes",
                        principalColumn: "LeaveTypeId",
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
                name: "IX_Roles_RoleType_Email",
                table: "Roles",
                columns: new[] { "RoleType", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaveLimits_LeaveTypeId_AssignedToUserEmail_ValidSince",
                table: "UserLeaveLimits",
                columns: new[] { "LeaveTypeId", "AssignedToUserEmail", "ValidSince" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLeaveLimits_LeaveTypeId_AssignedToUserEmail_ValidUntil",
                table: "UserLeaveLimits",
                columns: new[] { "LeaveTypeId", "AssignedToUserEmail", "ValidUntil" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserLeaveLimits");

            migrationBuilder.DropTable(
                name: "LeaveTypes");
        }
    }
}
