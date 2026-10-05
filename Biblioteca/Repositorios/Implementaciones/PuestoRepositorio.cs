using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class PuestoRepositorio : IPuestoRepositorio
    {
        private readonly IConexion _db;

        public PuestoRepositorio(IConexion db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Puesto>> ListarPorVehiculoAsync(long idVehiculo, CancellationToken ct = default)
        {
            return await _db.Puestos
                .AsNoTracking()
                .Include(p => p.CodigoPuesto)
                .Include(p => p.TipoPuesto)
                .Where(p => p.IdVehiculo == idVehiculo)
                .OrderBy(p => p.Id)
                .ToListAsync(ct);
        }

        public async Task<CodigoPuesto> ObtenerOCrearCodigoAsync(string descripcion, CancellationToken ct = default)
        {
            var existente = await _db.CodigosPuesto.FirstOrDefaultAsync(c => c.Descripcion == descripcion, ct);
            if (existente is not null)
                return existente;

            var nuevo = new CodigoPuesto { Descripcion = descripcion };
            _db.CodigosPuesto.Add(nuevo);
            await _db.SaveChangesAsync(ct);
            return nuevo;
        }

        public async Task<TipoPuesto> ObtenerOCrearTipoAsync(string descripcion, CancellationToken ct = default)
        {
            var existente = await _db.TiposPuesto.FirstOrDefaultAsync(t => t.Descripcion == descripcion, ct);
            if (existente is not null)
                return existente;

            var nuevo = new TipoPuesto { Descripcion = descripcion };
            _db.TiposPuesto.Add(nuevo);
            await _db.SaveChangesAsync(ct);
            return nuevo;
        }

        public async Task AgregarRangoAsync(IEnumerable<Puesto> puestos, CancellationToken ct = default)
        {
            _db.Puestos.AddRange(puestos);
            await _db.SaveChangesAsync(ct);
        }
    }
}