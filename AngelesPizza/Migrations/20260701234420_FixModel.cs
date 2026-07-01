using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngelesPizza.Migrations
{
    /// <inheritdoc />
    public partial class FixModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductModifiers_Products_ProductId",
                table: "ProductModifiers");

            migrationBuilder.DropIndex(
                name: "IX_ProductModifiers_ProductId",
                table: "ProductModifiers");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductModifiers");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "OrderDetails");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "OrderDetails",
                newName: "Price");

            migrationBuilder.AddColumn<int>(
                name: "ExtraCost",
                table: "OrderDetailModifiers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraCost",
                table: "OrderDetailModifiers");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "OrderDetails",
                newName: "UnitPrice");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductModifiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Total",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductModifiers_ProductId",
                table: "ProductModifiers",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductModifiers_Products_ProductId",
                table: "ProductModifiers",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
