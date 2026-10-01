using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class CiudadRepositorio : ICiudadRepositorio
    {
        private readonly IConexion _db;

        public CiudadRepositorio(IConexion db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Ciudad>> ListarAsync(CancellationToken ct = default)
        {
            return await _db.Ciudades
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .ToListAsync(ct);
        }

        
    }
}