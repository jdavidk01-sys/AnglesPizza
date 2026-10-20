using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AngelesPizza.Migrations
{
    /// <inheritdoc />
    public partial class AddBankToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BankId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SwiftCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Banks",
                columns: new[] { "Id", "CreatedAt", "IsActive", "Name", "SwiftCode" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Bancolombia", "COLCCO22" },
                    { 2, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Banco de Bogotá", "BOGOCO22" },
                    { 3, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Banco Santander", "BSANESCO" },
                    { 4, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "BBVA", "BBVAESCO" },
                    { 5, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Davivienda", "DAVICO22" },
                    { 6, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Banco Caja Social", "CAJACO22" },
                    { 7, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Banco Occidente", "OCCOCO22" },
                    { 8, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Banco Popular", "BOPOCO22" },
                    { 9, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Banco Falabella", "FALACO22" },
                    { 10, new DateTime(2024, 10, 20, 0, 0, 0, DateTimeKind.Utc), true, "Scotiabank", "SCBLESCO" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BankId",
                table: "Payments",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_Name",
                table: "Banks",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Banks_BankId",
                table: "Payments",
                column: "BankId",
                principalTable: "Banks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Banks_BankId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BankId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "Payments");
        }
    }
}
