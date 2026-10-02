using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class QuitarHoras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Viajes_Horas_IdHora",
                table: "Viajes");

            migrationBuilder.DropTable(
                name: "Horas");

            migrationBuilder.DropIndex(
                name: "IX_Viajes_IdHora",
                table: "Viajes");

            migrationBuilder.DropColumn(
                name: "IdHora",
                table: "Viajes");

            migrationBuilder.InsertData(
                table: "Ciudades",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1L, "Sonsón" },
                    { 2L, "La Unión" },
                    { 3L, "Rionegro" },
                    { 4L, "La Ceja" },
                    { 5L, "Medellín" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ciudades",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Ciudades",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Ciudades",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Ciudades",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Ciudades",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.AddColumn<long>(
                name: "IdHora",
                table: "Viajes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "Horas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValorHora = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Viajes_IdHora",
                table: "Viajes",
                column: "IdHora");

            migrationBuilder.CreateIndex(
                name: "IX_Horas_ValorHora",
                table: "Horas",
                column: "ValorHora",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Viajes_Horas_IdHora",
                table: "Viajes",
                column: "IdHora",
                principalTable: "Horas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
