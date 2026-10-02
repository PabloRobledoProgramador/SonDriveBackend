using Biblioteca.DTO.Ruta;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Biblioteca.Servicios.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Servicios.Implementaciones
{
    public class RutaServicio : IRutaServicio
    {
        private readonly IRutaRepositorio _repositorio;

        public RutaServicio(IRutaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<IEnumerable<RutaResponseDTO>> ListarAsync(CancellationToken ct = default)
        {
            var rutas = await _repositorio.ListarAsync(ct);
            return rutas.Select(ADto).ToList();
        }

        // Conversión manual Entidad -> DTO (sin AutoMapper)
        private static RutaResponseDTO ADto(Ruta r) => new()
        {
            Id = r.Id,
            CiudadOrigenId = r.CiudadOrigenId,
            CiudadOrigen = r.CiudadOrigen?.Nombre ?? string.Empty,
            CiudadDestinoId = r.CiudadDestinoId,
            CiudadDestino = r.CiudadDestino?.Nombre ?? string.Empty,
            PrecioBase = r.PrecioBase,
            DistanciaKm = r.DistanciaKm,
            TiempoEstimado = r.TiempoEstimado
        };
    }
}
