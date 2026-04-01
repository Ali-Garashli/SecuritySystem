using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecurityDashboard.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "SwitchedTime",
                value: new DateTime(2026, 3, 30, 21, 9, 59, 966, DateTimeKind.Local).AddTicks(4840));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "SystemStatus",
                keyColumn: "Id",
                keyValue: 1,
                column: "SwitchedTime",
                value: new DateTime(2026, 3, 30, 20, 41, 30, 85, DateTimeKind.Local).AddTicks(3200));
        }
    }
}
