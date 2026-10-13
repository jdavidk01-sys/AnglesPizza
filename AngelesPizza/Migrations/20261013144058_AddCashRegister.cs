using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngelesPizza.Migrations
{
    /// <inheritdoc />
    public partial class AddCashRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CashRegisterId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CashRegisters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitialAmount = table.Column<int>(type: "int", nullable: false),
                    TotalCashSales = table.Column<int>(type: "int", nullable: false),
                    TotalTransferSales = table.Column<int>(type: "int", nullable: false),
                    TotalCardSales = table.Column<int>(type: "int", nullable: false),
                    FinalCashExpected = table.Column<int>(type: "int", nullable: false),
                    FinalCashReal = table.Column<int>(type: "int", nullable: true),
                    CashDifference = table.Column<int>(type: "int", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CashRegisterId",
                table: "Payments",
                column: "CashRegisterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CashRegisters_CashRegisterId",
                table: "Payments",
                column: "CashRegisterId",
                principalTable: "CashRegisters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CashRegisters_CashRegisterId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "CashRegisters");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CashRegisterId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CashRegisterId",
                table: "Payments");
        }
    }
}
