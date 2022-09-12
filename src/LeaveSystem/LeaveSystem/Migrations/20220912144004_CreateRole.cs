using System;
using LeaveSystem.Db;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LeaveSystem.Migrations
{
    public partial class CreateRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<FederatedUser>(
                name: "User",
                table: "UserLeaveLimits",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(FederatedUser),
                oldType: "jsonb");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.AlterColumn<FederatedUser>(
                name: "User",
                table: "UserLeaveLimits",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(FederatedUser),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
