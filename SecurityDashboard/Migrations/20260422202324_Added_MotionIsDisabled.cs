using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityDashboard.Migrations
{
    /// <inheritdoc />
    public partial class Added_MotionIsDisabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MotionIsDisabled",
                table: "SystemStatus",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "SystemStatus",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MotionIsDisabled", "SwitchedTime" },
                values: new object[] { false, new DateTime(2026, 4, 23, 0, 23, 23, 981, DateTimeKind.Local).AddTicks(8500) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MotionIsDisabled",
                table: "SystemStatus");

            migrationBuilder.UpdateData(
                table: "SystemStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "SwitchedTime",
                value: new DateTime(2026, 4, 6, 12, 28, 36, 460, DateTimeKind.Local).AddTicks(8138));
        }
    }
}
