using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngelesPizza.Migrations
{
    /// <inheritdoc />
    public partial class AddProductXModifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductModifierProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductModifierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModifierProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModifierProducts_ProductModifiers_ProductModifierId",
                        column: x => x.ProductModifierId,
                        principalTable: "ProductModifiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductModifierProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductModifierProducts_ProductId",
                table: "ProductModifierProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModifierProducts_ProductModifierId",
                table: "ProductModifierProducts",
                column: "ProductModifierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductModifierProducts");
        }
    }
}
