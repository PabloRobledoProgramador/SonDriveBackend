using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class ViajeRepositorio : IViajeRepositorio
    {
        private readonly IConexion _db;

        public ViajeRepositorio(IConexion db)
        {
            _db = db;
        }

        private IQueryable<Viaje> ConDetalle()
        {
            return _db.Viajes
                .AsNoTracking()
                .AsSplitQuery()
                .Include(v => v.CiudadOrigen)
                .Include(v => v.CiudadDestino)
                .Include(v => v.Ocupaciones)
                .Include(v => v.Vehiculo).ThenInclude(x => x.Usuario)
                .Include(v => v.Vehiculo).ThenInclude(x => x.Puestos).ThenInclude(p => p.CodigoPuesto)
                .Include(v => v.Vehiculo).ThenInclude(x => x.Puestos).ThenInclude(p => p.TipoPuesto);
        }

        public async Task<IReadOnlyList<Viaje>> BuscarAsync(
            long? origenId, long? destinoId, DateTime? fecha, TimeSpan? hora, CancellationToken ct = default)
        {
            var ahora = DateTime.Now;
            var query = ConDetalle().Where(v => v.FechaSalida >= ahora);

            if (origenId.HasValue)
                query = query.Where(v => v.CiudadOrigenId == origenId.Value);

            if (destinoId.HasValue)
                query = query.Where(v => v.CiudadDestinoId == destinoId.Value);

            if (fecha.HasValue)
            {
                var inicio = fecha.Value.Date;
                var fin = inicio.AddDays(1);
                query = query.Where(v => v.FechaSalida >= inicio && v.FechaSalida < fin);
            }

            if (hora.HasValue)
                query = query.Where(v => v.FechaSalida.TimeOfDay == hora.Value);

            return await query
                .OrderBy(v => v.FechaSalida)
                .ThenBy(v => v.Id)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Viaje>> ListarPorUsuarioAsync(long idUsuario, CancellationToken ct = default)
        {
            var ahora = DateTime.Now;

            return await ConDetalle()
                .Where(v => v.Vehiculo.IdUsuario == idUsuario && v.FechaSalida >= ahora)
                .OrderBy(v => v.FechaSalida)
                .ToListAsync(ct);
        }

        public async Task<Viaje?> ObtenerPorIdAsync(long id, bool incluirDetalle = false, CancellationToken ct = default)
        {
            IQueryable<Viaje> query = incluirDetalle ? ConDetalle() : _db.Viajes.AsNoTracking();
            return await query.FirstOrDefaultAsync(v => v.Id == id, ct);
        }

        public async Task<bool> ExisteViajeVehiculoAsync(long idVehiculo, DateTime fechaSalida, long? excluirId = null, CancellationToken ct = default)
        {
            return await _db.Viajes.AnyAsync(
                v => v.IdVehiculo == idVehiculo
                    && v.FechaSalida == fechaSalida
                    && (excluirId == null || v.Id != excluirId),
                ct);
        }

        public async Task<bool> TieneOcupacionesAsync(long id, CancellationToken ct = default)
        {
            return await _db.Ocupaciones.AnyAsync(o => o.IdViaje == id, ct);
        }

        public async Task<Viaje> CrearAsync(Viaje viaje, CancellationToken ct = default)
        {
            _db.Viajes.Add(viaje);
            await _db.SaveChangesAsync(ct);
            return viaje;
        }

        public async Task<Viaje> ActualizarAsync(Viaje viaje, CancellationToken ct = default)
        {
            _db.Viajes.Update(viaje);
            await _db.SaveChangesAsync(ct);
            return viaje;
        }

        public async Task<bool> EliminarAsync(long id, CancellationToken ct = default)
        {
            var viaje = await _db.Viajes.FindAsync(new object[] { id }, ct);
            if (viaje is null)
                return false;

            _db.Viajes.Remove(viaje);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}