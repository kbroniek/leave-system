using System;
using LeaveSystem.Db;
using LeaveSystem.Db.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveSystem.Migrations
{
    public partial class CreateUserLeaveLimit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<FederatedUser[]>(
                name: "Users",
                table: "Departments",
                type: "jsonb",
                nullable: false,
                defaultValue: new FederatedUser[0],
                oldClrType: typeof(FederatedUser[]),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "UserLeaveLimits",
                columns: table => new
                {
                    UserLeaveLimitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Limit = table.Column<TimeSpan>(type: "interval", nullable: false),
                    OverdueLimit = table.Column<TimeSpan>(type: "interval", nullable: true),
                    User = table.Column<FederatedUser>(type: "jsonb", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidSince = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "IX_UserLeaveLimits_LeaveTypeId",
                table: "UserLeaveLimits",
                column: "LeaveTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserLeaveLimits");

            migrationBuilder.AlterColumn<FederatedUser[]>(
                name: "Users",
                table: "Departments",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(FederatedUser[]),
                oldType: "jsonb");
        }
    }
}
