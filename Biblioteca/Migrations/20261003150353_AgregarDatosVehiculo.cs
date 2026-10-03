using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Biblioteca.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoServicio",
                table: "Vehiculos",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Kilometraje",
                table: "Vehiculos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VencimientoLicencia",
                table: "Vehiculos",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "VencimientoSoat",
                table: "Vehiculos",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "VencimientoTecnomecanica",
                table: "Vehiculos",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "Rutas",
                columns: new[] { "Id", "CiudadDestinoId", "CiudadOrigenId", "DistanciaKm", "PrecioBase", "TiempoEstimado" },
                values: new object[,]
                {
                    { 1L, 2L, 1L, 37.0, 12000m, "45 min" },
                    { 2L, 3L, 1L, 70.0, 30000m, "1 h 45 min" },
                    { 3L, 4L, 1L, 65.0, 25000m, "1 h 30 min" },
                    { 4L, 5L, 1L, 115.0, 35000m, "2 h" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rutas",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "Rutas",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "Rutas",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "Rutas",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DropColumn(
                name: "FechaUltimoServicio",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Kilometraje",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "VencimientoLicencia",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "VencimientoSoat",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "VencimientoTecnomecanica",
                table: "Vehiculos");
        }
    }
}
