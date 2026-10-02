using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class RutaRepositorio : IRutaRepositorio
    {
        private readonly IConexion _db;

        public RutaRepositorio(IConexion db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Ruta>> ListarAsync(CancellationToken ct = default)
        {
            // Include porque CiudadOrigen y CiudadDestino son opcionales en Ruta
            return await _db.Rutas
                .AsNoTracking()
                .Include(r => r.CiudadOrigen)
                .Include(r => r.CiudadDestino)
                .OrderBy(r => r.DistanciaKm)
                .ThenBy(r => r.Id)
                .ToListAsync(ct);
        }
    }
}
