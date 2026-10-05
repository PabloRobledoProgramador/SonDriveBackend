using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class OcupacionRepositorio : IOcupacionRepositorio
    {
        private readonly IConexion _db;

        public OcupacionRepositorio(IConexion db)
        {
            _db = db;
        }

        private IQueryable<Ocupacion> ConDetalle()
        {
            return _db.Ocupaciones
                .AsNoTracking()
                .Include(o => o.Pasajero)
                .Include(o => o.Puesto).ThenInclude(p => p.CodigoPuesto)
                .Include(o => o.Puesto).ThenInclude(p => p.TipoPuesto);
        }

        public async Task<IReadOnlyList<Ocupacion>> ListarPorViajeAsync(long idViaje, CancellationToken ct = default)
        {
            return await ConDetalle()
                .Where(o => o.IdViaje == idViaje && o.Ocupado)
                .OrderBy(o => o.IdPuesto)
                .ToListAsync(ct);
        }

        public async Task<Ocupacion?> ObtenerAsync(long idViaje, long idPuesto, CancellationToken ct = default)
        {
            return await ConDetalle()
                .FirstOrDefaultAsync(o => o.IdViaje == idViaje && o.IdPuesto == idPuesto, ct);
        }

        // Pasajero + ocupación se guardan en un solo SaveChanges (una transacción).
        public async Task<Ocupacion> CrearAsync(Ocupacion ocupacion, CancellationToken ct = default)
        {
            _db.Ocupaciones.Add(ocupacion);
            await _db.SaveChangesAsync(ct);
            return ocupacion;
        }

        // Corrige los datos del pasajero sin tocar la ocupación (el puesto sigue ocupado).
        public async Task ActualizarPasajeroAsync(Pasajero pasajero, CancellationToken ct = default)
        {
            _db.Pasajeros.Update(pasajero);
            await _db.SaveChangesAsync(ct);
        }

        // Libera: borra la ocupación y, si el pasajero no tiene más, también el pasajero.
        public async Task EliminarAsync(long idOcupacion, CancellationToken ct = default)
        {
            var ocupacion = await _db.Ocupaciones
                .Include(o => o.Pasajero)
                .FirstOrDefaultAsync(o => o.Id == idOcupacion, ct);

            if (ocupacion is null)
                return;

            var pasajero = ocupacion.Pasajero;
            _db.Ocupaciones.Remove(ocupacion);
            await _db.SaveChangesAsync(ct);

            var tieneMas = await _db.Ocupaciones.AnyAsync(o => o.IdPasajero == pasajero.Id, ct);
            if (!tieneMas)
            {
                _db.Pasajeros.Remove(pasajero);
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}