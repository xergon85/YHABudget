using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YHABudget.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSalarySettingsSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SalarySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SalarySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2025, 12, 5, 8, 4, 4, 884, DateTimeKind.Local).AddTicks(9836));
        }
    }
}
