using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class VehiculoRepositorio : IVehiculoRepositorio
    {
        private readonly IConexion _db;

        public VehiculoRepositorio(IConexion db)
        {
            _db = db;
        }

        private IQueryable<Vehiculo> ConDetalle()
        {
            return _db.Vehiculos
                .AsNoTracking()
                .Include(v => v.Usuario)
                .Include(v => v.Fallas.Where(f => !f.EstaResuelta));
        }

        public async Task<IReadOnlyList<Vehiculo>> ListarAsync(CancellationToken ct = default)
        {
            return await ConDetalle()
                .OrderBy(v => v.Placa)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Vehiculo>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default)
        {
            return await ConDetalle()
                .Where(v => v.IdUsuario == idUsuario)
                .OrderBy(v => v.Placa)
                .ToListAsync(ct);
        }

        public async Task<Vehiculo?> ObtenerPorIdAsync(long id, bool incluirDetalle = false, CancellationToken ct = default)
        {
            IQueryable<Vehiculo> query = incluirDetalle ? ConDetalle() : _db.Vehiculos.AsNoTracking();
            return await query.FirstOrDefaultAsync(v => v.Id == id, ct);
        }

        public async Task<bool> ExistePlacaAsync(string placa, long? excluirId = null, CancellationToken ct = default)
        {
            return await _db.Vehiculos.AnyAsync(
                v => v.Placa == placa && (excluirId == null || v.Id != excluirId),
                ct);
        }

        public async Task<bool> TieneViajesAsync(long id, CancellationToken ct = default)
        {
            return await _db.Viajes.AnyAsync(v => v.IdVehiculo == id, ct);
        }

        public async Task<Vehiculo> CrearAsync(Vehiculo vehiculo, CancellationToken ct = default)
        {
            _db.Vehiculos.Add(vehiculo);
            await _db.SaveChangesAsync(ct);
            return vehiculo;
        }

        public async Task<Vehiculo> ActualizarAsync(Vehiculo vehiculo, CancellationToken ct = default)
        {
            _db.Vehiculos.Update(vehiculo);
            await _db.SaveChangesAsync(ct);
            return vehiculo;
        }

        public async Task<bool> EliminarAsync(long id, CancellationToken ct = default)
        {
            var vehiculo = await _db.Vehiculos.FindAsync(new object[] { id }, ct);
            if (vehiculo is null)
                return false;

            _db.Vehiculos.Remove(vehiculo);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}