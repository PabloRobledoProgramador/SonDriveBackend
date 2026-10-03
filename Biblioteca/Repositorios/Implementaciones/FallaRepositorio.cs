using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class FallaRepositorio : IFallaRepositorio
    {
        private readonly IConexion _db;

        public FallaRepositorio(IConexion db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Falla>> ListarPorVehiculoAsync(long idVehiculo, CancellationToken ct = default)
        {
            return await _db.Fallas
                .AsNoTracking()
                .Where(f => f.VehiculoId == idVehiculo)
                .OrderByDescending(f => f.Fecha)
                .ToListAsync(ct);
        }

        public async Task<Falla?> ObtenerPorIdAsync(long id, CancellationToken ct = default)
        {
            return await _db.Fallas
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id, ct);
        }

        public async Task<Falla> CrearAsync(Falla falla, CancellationToken ct = default)
        {
            _db.Fallas.Add(falla);
            await _db.SaveChangesAsync(ct);
            return falla;
        }

        public async Task<Falla> ActualizarAsync(Falla falla, CancellationToken ct = default)
        {
            _db.Fallas.Update(falla);
            await _db.SaveChangesAsync(ct);
            return falla;
        }

        public async Task<bool> EliminarAsync(long id, CancellationToken ct = default)
        {
            var falla = await _db.Fallas.FindAsync(new object[] { id }, ct);
            if (falla is null)
                return false;

            _db.Fallas.Remove(falla);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}